using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACD.Domain.Entities;
using SACD.Domain.Rules;

namespace SACD.Infrastructure.Rpa
{
    public interface IRpaEngine
    {
        Task<RpaRunEntity> ExecuteScriptAsync(
            RpaRunEntity run,
            IReadOnlyList<ScriptStepEntity> steps,
            IReadOnlyDictionary<string, string> variableBindings,
            bool stepByStepMode,
            Func<ScriptStepEntity, Task<bool>>? onPauseForUser = null,
            CancellationToken ct = default);
    }

    public class RpaEngine : IRpaEngine
    {
        public async Task<RpaRunEntity> ExecuteScriptAsync(
            RpaRunEntity run,
            IReadOnlyList<ScriptStepEntity> steps,
            IReadOnlyDictionary<string, string> variableBindings,
            bool stepByStepMode,
            Func<ScriptStepEntity, Task<bool>>? onPauseForUser = null,
            CancellationToken ct = default)
        {
            run.State = "Running";

            for (int i = run.CurrentStep; i < steps.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var step = steps[i];
                run.CurrentStep = step.Ordinal;

                if (step.Action == "PauseForUser")
                {
                    run.State = "AwaitingUser";
                    if (onPauseForUser != null)
                    {
                        bool resume = await onPauseForUser(step);
                        if (!resume)
                        {
                            run.State = "Paused";
                            return run;
                        }
                    }
                    else
                    {
                        return run;
                    }
                }

                // Simulate step execution: Prepared -> Dispatched -> Verified
                BusinessRuleValidator.ValidateRpaStepTransition("Prepared", "Dispatched");

                // Execute action
                await Task.Delay(stepByStepMode ? 250 : 50, ct);

                BusinessRuleValidator.ValidateRpaStepTransition("Dispatched", "Verified");

                if (stepByStepMode && i < steps.Count - 1)
                {
                    run.State = "Paused";
                    run.CurrentStep = i + 1;
                    return run;
                }
            }

            run.State = "Succeeded";
            return run;
        }
    }
}
