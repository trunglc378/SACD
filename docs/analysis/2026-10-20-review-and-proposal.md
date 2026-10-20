> Báo cáo này mô tả commit nền trước sửa. Xem [bản sửa và chạy thử mới](/D:/Project/SACD/docs/09_SACD_Ban_sua_va_chay_thu_2026-10-20.md) để biết hiện trạng sau sửa. Các liên kết code bên dưới trỏ commit nền.

# SACD — Rà soát mã nguồn, tài liệu và phương án tự động hóa

Ngày đánh giá: **20/10/2026**, múi giờ Việt Nam. Commit nền: `293323cd020800232d9102ace9d6c90ac6fa0b64`.

Đây là báo cáo đánh giá và đề xuất, chưa phải kết quả nghiệm thu hoặc bản triển khai tính năng mới.

## 1. Kết luận và phạm vi

**Tiếp tục phát triển SACD thành trợ lý văn thư desktop điều phối hai kho độc lập; xây automation chuyên biệt cho hai website; bổ sung PDF/OCR và trích xuất có căn cứ; thử AI local cho tham mưu sau khi luồng lõi ổn định.** Giữ .NET/WPF, SQLite, kho bản gốc, lịch sử và cơ chế USB; cải tiến từng phần thay vì viết lại toàn bộ.

Máy 1 và máy 2 không có Internet, không liên thông, nhưng mỗi máy truy cập website của mình qua một mạng nội bộ riêng. Mỗi máy vận hành độc lập; hồ sơ và dữ liệu đã kiểm tra được chuyển qua USB an toàn. Không đề xuất máy chủ dùng chung hoặc một AI server nối hai mạng.

Core i3 thế hệ 10/RAM 8 GB đủ để lựa chọn thử nghiệm ứng dụng và automation nhẹ; chưa phải bằng chứng đáp ứng thông lượng OCR hoặc AI. Chưa biết mã CPU cụ thể, RAM khả dụng, SSD/HDD, OS, trình duyệt, số trang/ngày và chất lượng scan. Không đưa ra cam kết tốc độ hoặc tỷ lệ tự động hóa trước đo.

Đánh giá bao gồm: README; phiếu đăng ký và Report 1 v1.0; chỉ mục, SRS, SDS, Use Case, ERD, RTM v1.1; quy trình hợp nhất v1.2; hướng dẫn cài đặt/Docker; báo cáo kiểm thử cũ; các lớp Domain/Application; luồng WPF chính và luồng v1.2; persistence, storage, transfer, text, RPA, Word, services; CLI/Docker; mã kiểm thử và TRX lưu trong kho. Các tài liệu liên kết nhưng không có trong checkout không được coi là đã đọc.

**Giới hạn kiểm chứng:** không có .NET SDK trong môi trường đánh giá, nên chưa build/chạy xUnit/WPF/UIA/Docker. Có chạy kiểm tra tài liệu, tạo DDL trên SQLite bộ nhớ và thực thi một số SQL trích từ nguồn bằng Python/SQLite 3.37.2. Đây không phải chạy C# service hay kiểm thử website nội bộ. Không truy cập website nghiệp vụ, máy in hoặc USB thật. Không sửa mã ứng dụng hay dữ liệu nghiệp vụ trong đợt đánh giá này.

## 2. Phân biệt bằng chứng và mục tiêu thiết kế

| Nhóm | Kết quả đọc/kiểm tra | Ý nghĩa |
|---|---|---|
| Bộ tài liệu | Có 13 Markdown trong `docs` trước báo cáo này; chỉ mục tuyên bố bộ 19 tài liệu thiết kế nhưng nhiều tệp không nằm trong checkout | Cần hoàn thiện bộ tài liệu đang có; không dẫn kết quả kiểm tra bộ khác làm bằng chứng cho checkout này |
| Liên kết | Kiểm tra tự động thấy 38 lượt liên kết tương đối trỏ đến đích không có, gồm bản sao chỉ mục trong `Review` | Một số là sai vị trí tương đối, một số là thiếu tài liệu; không có nghĩa 38 tài liệu độc lập bị thiếu |
| Schema | 53 bảng nghiệp vụ khai báo; 59 bảng thực tế khi tính FTS và shadow; `integrity_check=ok`, bật FK | Chứng minh DDL tạo được trên runtime kiểm tra, chưa chứng minh đủ invariant nghiệp vụ |
| xUnit | 23 phương thức `[Fact]`; TRX lưu trong Git ghi 23/23 đạt, ngày 16/10/2026 | Bằng chứng lịch sử, chưa tái chạy tại commit trên môi trường hiện tại |
| Smoke | Mã smoke tạo WPF form giả, thử UIA bốn ô và dừng trước Lưu | Chưa kiểm chứng tương thích website thật, in thật hoặc USB thật |
| Báo cáo cũ | Tài liệu trong `Gemini test` ghi nhiều NFR đạt 100%, phiên bản dependency và số test khác hiện tại | Không dùng làm căn cứ nghiệm thu đầy đủ 117 FR/24 NFR |

Nguồn kiểm chứng máy đọc được: [2026-10-20-verification.json](/D:/Project/SACD/docs/analysis/2026-10-20-verification.json). Script tái chạy: [verify_repository.py](/D:/Project/SACD/docs/analysis/verify_repository.py). Script chỉ đọc nguồn, chạy SQL trong bộ nhớ và ghi báo cáo JSON; không mở/sửa SQLite nghiệp vụ có sẵn.

## 3. Kiến trúc thực tế và phần nên giữ

Luồng văn thư v1.2 hiện đi qua `CorrespondenceViewModel` → `CorrespondenceService` → `CorrespondenceCase`/event/import cùng các bảng Document/Text. Các tab kỹ thuật khác tiếp tục đi qua `MainViewModel` và `ServicesPart1/2`. Hai nhánh chưa có một hợp đồng nghiệp vụ thống nhất cho mọi thao tác.

Các phần có giá trị để tiếp tục:

- Bản gốc lưu theo hash; không lấy tên tệp làm định danh duy nhất.
- Kho `Intake` và `Management` tách biệt; khóa một phiên ghi ở ứng dụng.
- Luồng v1.2 kiểm soát thứ tự, `rowVersion`, căn cứ thao tác và audit trong giao dịch khi cập nhật case.
- Đề xuất người nhận và người được giao theo phê duyệt là hai trường khác nhau.
- Gói USB v1.2 mang tệp gốc, metadata và text; giữ nguồn, chống nhập lặp/xung đột.
- UIA thật kiểm tra executable, cửa sổ, trường duy nhất, đọc lại giá trị; dừng trước Lưu.
- DOCX/QR, tìm kiếm tiếng Việt, tính hạn và backup dùng SQLite Backup API đã có nền tảng.

Không suy ra mọi thao tác ở các service cũ đã có các bảo đảm của luồng v1.2. Các nhận xét về service cũ dưới đây không đồng nghĩa giao diện v1.2 đang gọi các nhánh đó.

## 4. Các phát hiện và hành động đề nghị

Ưu tiên **P0**: sửa hoặc khóa nhánh liên quan trước dùng dữ liệu thật cho chức năng đó. **P1**: cần cho bản thí điểm tự động hóa. **P2**: hoàn thiện vận hành/mở rộng. Đây là thứ tự đề xuất, không phải kết quả chấm điểm bảo mật.

| Mã | Phát hiện có căn cứ | Tác động và hành động | Ưu tiên |
|---|---|---|---|
| F01 | `DocumentTextExtractor` đọc bytes PDF thành UTF-8, chia “trang” theo số block; DOCX chỉ lấy paragraph trực tiếp trong body | Không dùng kết quả này để lọc/AI trên PDF thật. Thay bằng PDF parser và OCR theo trang; đọc bảng/header/footer DOCX; đánh dấu lỗi hoặc thiếu nội dung | P0 với PDF tự động |
| F02 | `OcrAsync` chỉ gọi `ExtractAsync`; extractor không nhận engine OCR thật; `SpecificPages`/language chưa điều phối OCR | Xây OCR worker có kết quả theo trang, timeout/hủy/checkpoint. Trước đó hiển thị rõ chưa hỗ trợ | P1 |
| F03 | `ReceiveMailAsync` chỉ nhập các đường dẫn đính kèm đã có; v1.2 dùng hộp chọn tệp | Chưa tải từ website. Xây connector tiếp nhận web, giữ ID nguồn/URL/đơn vị gửi và trạng thái tải | P1 |
| F04 | `RpaEngine` dùng `Task.Delay`; recorder Start/Stop chỉ trả ID; `ControlAsync` đổi trạng thái DB chứ chưa điều khiển worker | Không nghiệm thu ghi/phát thao tác thật từ các test mô phỏng. Giữ trong chế độ kỹ thuật, phát triển adapter cụ thể trước recorder tổng quát | P1 |
| F05 | `ExternalEntryService.FillAsync` cũ tạo `Verified` và mã QLVB ngẫu nhiên; `PrintService.SubmitAsync` tạo mã `SPOOL-*` nội bộ | Các nhánh chưa tích hợp phải trả `NotConfigured`/mô phỏng, không trả kết quả nghiệp vụ thành công. Không lấy mã giả làm căn cứ | P0 với các nhánh cũ |
| F06 | UIA v1.2 chỉ có Number/Summary/Issuer/DueDate; chỉ cho chạy khi `Imported` | Xây adapter web đúng biểu mẫu, bổ sung ngày/người ký/loại/nơi nhận/tệp nếu đích yêu cầu. Thêm cập nhật chỉ đạo sau duyệt và đối soát hồ sơ đích | P1 |
| F07 | `CorrespondenceCase` gắn duy nhất một Document; tiếp nhận trùng theo hash trả case cũ, không tạo nguồn tiếp nhận mới | Một công văn có nhiều phụ lục và một tệp được gửi nhiều lần cần mô hình riêng. Tách bản gốc trùng bytes khỏi lần tiếp nhận và hồ sơ nghiệp vụ | P1 |
| F08 | Case state ở JSON, task ở `TaskItem`, lịch sử ở hai hệ bảng; báo cáo nháp UI đếm `ImportedDocuments` của phiên | Có thể không phản ánh đầy đủ kho sau mở lại hoặc luồng v1.2. Xác định một aggregate hồ sơ, quan hệ nhiệm vụ và truy vấn báo cáo từ DB theo kỳ | P1 |
| F09 | `SaveReviewAsync` sửa text trong cùng revision; không chặn sửa revision đã có nguồn tham chiếu; không đánh stale ngay ở nhánh sửa text | Căn cứ cũ có thể trỏ ID không đổi nhưng text đổi. Sửa tạo revision mới hoặc khóa revision đã tham chiếu; cập nhật nguồn phụ thuộc trong cùng giao dịch | P0 với bằng chứng/báo cáo |
| F10 | FTS thêm mỗi revision nhưng truy vấn không giới hạn current revision; SQL probe trả revision 1 dù Document trỏ revision 2 | Lọc current mặc định; lịch sử chọn riêng; rebuild chỉ mục có phiên bản và kiểm thử tránh kết quả cũ/trùng | P1 |
| F11 | `FactService` bỏ qua kiểm tra metric/kỳ/scope và quyết định dedup khi tổng hợp; Stock lấy `values[0]`; Ratio ép decimal sang long; Cumulative cộng như Flow | Stock 10 cũ/25 mới cho 10 hoặc 25 tùy thứ tự request; Ratio 1,5/2,5 bị biến thành 1/2; số lũy kế có thể cộng chồng. Sửa trước phát hành module tổng hợp | P0 với số liệu |
| F12 | `ReportService.ReviewAsync` đổi state mà chưa kiểm tra đủ claim/evidence/current source/expected version; export chỉ chặn state và stale | SQL probe chuyển Reviewed khi claim `checked=0`, không có ClaimEvidence. UI hiện chặn bản trình nhưng service chưa đủ bảo vệ. Bổ sung validation tại service trước mở chức năng | P0 với bản trình |
| F13 | Đăng ký mẫu Word chưa insert FileObject mới trước FK TemplateVersion; export dựng DOCX mới, không dùng file mẫu; `sourceHash='HASH'`, aggregation IDs chưa được lưu theo hợp đồng | Chức năng mẫu/tổng hợp chưa đạt mô tả thiết kế. Triển khai template/tag thật, hash thật và ReportAggregation; kiểm tra bố cục trên mẫu đơn vị | P1/P2 theo phạm vi phát hành |
| F14 | `RestoreAsync` chỉ giải nén có overwrite rồi báo `IntegrityOk=true` | Chưa xác minh hash, integrity/FK, schema/chủ kho hay đích mới. Phục hồi vào staging/kho mới, kiểm tra rồi mới công bố thành công; thử phục hồi có mở tệp | P0 với phục hồi |
| F15 | `ValidateWorkspace` chỉ kiểm tra OwnerSid không rỗng và schema; chưa đối chiếu SID người mở/ACL theo SRS | Không tuyên bố đã có kiểm soát quyền đầy đủ. Chọn chính sách chủ kho/nhóm được cấp quyền, thực thi và thử với hai tài khoản Windows | P0 trước dùng dữ liệu thật |
| F16 | `List()` đọc/deserialise toàn bộ JSON; `Get(id)` gọi List; timer hạn đọc lại tất cả; UI chưa có hàng đợi và cơ chế hủy OCR thực | Thêm truy vấn theo ID, phân trang, chỉ mục trường thường lọc; một worker nặng và tiến độ/hủy. Đo UI trên kho lớn | P1 |
| F17 | Có hai định dạng USB: manifest cũ và gói case mới; importer cũ không tái dựng Document/Dossier; exporter mới nạp cả tệp vào RAM | Chuẩn hóa hợp đồng gói có version, nhiều tài liệu và import staging theo luồng. Giữ adapter đọc gói cũ; không gộp định dạng âm thầm | P1 |
| F18 | Luồng hiện có khóa metadata sau xác nhận; chưa có sửa sai/trả lại/gia hạn/điều chỉnh sau duyệt hoàn chỉnh; tasks phụ tạo mặc định hạn +3 ngày | Bổ sung sự kiện và phiên bản nghiệp vụ; hạn chưa rõ để Unknown; ngưỡng cảnh báo cấu hình thống nhất thay vì 24h/3 ngày ở hai luồng | P1 |
| F19 | Schema mở rộng additive vẫn version 1; thiếu migration tổng quát, build offline và rollback được kiểm chứng | Thêm migration/version/package compatibility; snapshot trước nâng cấp và thử rollback ở kho sao chép | P1 |
| F20 | CLI giữ writer lock rồi chờ; Docker healthcheck chạy CLI `--diag` mở cùng kho qua `WorkspaceService` trước khi chẩn đoán | Theo đường gọi, healthcheck sẽ tranh writer lock với tiến trình chính. Cần probe chỉ đọc, không mở phiên ghi. Docker là nhánh thử kỹ thuật, không ưu tiên trên máy nghiệp vụ 8 GB | P2 |
| F21 | Chỉ mục tài liệu thiếu đích; báo cáo kiểm thử cũ khẳng định NFR 100% khi OCR/UIA/web/hiệu năng chưa có bằng chứng tương ứng | Cập nhật chỉ mục, RTM và báo cáo; tách Designed/Implemented/Unit tested/Integrated/UAT. Không quy đổi 23 test thành % yêu cầu hoàn thành | P1 |

### Các điểm nguồn để đối chiếu

- Tiếp nhận, nguồn và OCR: [ServicesPart1.cs](https://github.com/trunglc378/SACD/blob/293323cd020800232d9102ace9d6c90ac6fa0b64/src/SACD.Infrastructure/Services/ServicesPart1.cs#L147), [OcrAsync](https://github.com/trunglc378/SACD/blob/293323cd020800232d9102ace9d6c90ac6fa0b64/src/SACD.Infrastructure/Services/ServicesPart1.cs#L385), [PDF fallback](https://github.com/trunglc378/SACD/blob/293323cd020800232d9102ace9d6c90ac6fa0b64/src/SACD.Infrastructure/Text/DocumentTextExtractor.cs#L183).
- Luồng case và USB mới: [CorrespondenceService.cs](https://github.com/trunglc378/SACD/blob/293323cd020800232d9102ace9d6c90ac6fa0b64/src/SACD.Infrastructure/Services/CorrespondenceService.cs#L55); UIA: [ControlledFormFiller.cs](https://github.com/trunglc378/SACD/blob/293323cd020800232d9102ace9d6c90ac6fa0b64/src/SACD.Desktop/ViewModels/ControlledFormFiller.cs#L23).
- Kết quả mô phỏng: [PrintService](https://github.com/trunglc378/SACD/blob/293323cd020800232d9102ace9d6c90ac6fa0b64/src/SACD.Infrastructure/Services/ServicesPart2.cs#L60), [ExternalEntryService](https://github.com/trunglc378/SACD/blob/293323cd020800232d9102ace9d6c90ac6fa0b64/src/SACD.Infrastructure/Services/ServicesPart2.cs#L459), [RpaEngine](https://github.com/trunglc378/SACD/blob/293323cd020800232d9102ace9d6c90ac6fa0b64/src/SACD.Infrastructure/Rpa/RpaEngine.cs#L58).
- Số liệu: [AggregateAsync](https://github.com/trunglc378/SACD/blob/293323cd020800232d9102ace9d6c90ac6fa0b64/src/SACD.Infrastructure/Services/ServicesPart2.cs#L825); rà soát báo cáo: [ReviewAsync](https://github.com/trunglc378/SACD/blob/293323cd020800232d9102ace9d6c90ac6fa0b64/src/SACD.Infrastructure/Services/ServicesPart2.cs#L1069).
- Phục hồi: [RestoreAsync](https://github.com/trunglc378/SACD/blob/293323cd020800232d9102ace9d6c90ac6fa0b64/src/SACD.Infrastructure/Services/ServicesPart2.cs#L1526); quyền: [ValidateWorkspace](https://github.com/trunglc378/SACD/blob/293323cd020800232d9102ace9d6c90ac6fa0b64/src/SACD.Domain/Rules/BusinessRuleValidator.cs#L25).
- Mẫu và báo cáo UI: [MainViewModel.cs](https://github.com/trunglc378/SACD/blob/293323cd020800232d9102ace9d6c90ac6fa0b64/src/SACD.Desktop/ViewModels/MainViewModel.cs#L390); schema: [DatabaseInitializer.cs](https://github.com/trunglc378/SACD/blob/293323cd020800232d9102ace9d6c90ac6fa0b64/src/SACD.Infrastructure/Persistence/DatabaseInitializer.cs#L8).

Các SQL probe chỉ chứng minh hành vi cụ thể của SQL/schema trên dữ liệu giả. Nhận định service đầy đủ dựa thêm vào đọc đường gọi; chưa thay integration test C#.

## 5. Thiết kế đề xuất cho quy trình hai máy

### Máy 1 — tiếp nhận và chuẩn bị

1. Cán bộ mở phiên đăng nhập website tiếp nhận. Connector xác minh đúng website, tài khoản/môi trường và trang danh sách.
2. Đọc mục văn bản đã chọn hoặc danh sách chờ theo cấu hình; lưu ID nguồn, ngày nhận, đơn vị chuyển, nội dung mô tả và danh sách tệp. Phân biệt thông tin trên website với thông tin trong văn bản.
3. Tải vào staging, kiểm tra tải hoàn tất, định dạng/kích thước, rồi lưu bản gốc và hash. Một lỗi phụ lục không được làm cả hồ sơ trông như đủ tệp.
4. PDF có text dùng parser; trang scan render rồi OCR. Với PDF hỗn hợp, xét từng trang và chất lượng text; không chỉ thử có chuỗi text hay không. DOCX đọc paragraph/bảng/header/footer theo thứ tự phù hợp.
5. Trích xuất số/ký hiệu, loại, ngày ban hành, đơn vị ban hành, người ký/chức vụ, trích yếu, nội dung yêu cầu, hạn và dấu hiệu khẩn/mật. Quy tắc và danh mục trước; AI tùy chọn hỗ trợ trường khó. Không thấy dữ liệu thì Unknown; không suy “Không mật” từ việc không tìm được dấu.
6. Màn hình đối chiếu hiển thị bản gốc cạnh biểu mẫu. Mỗi trường có giá trị gợi ý, nguồn trang/đoạn/vùng, phương pháp trích xuất và trạng thái đã kiểm tra. Cán bộ sửa nhanh, xác nhận theo nhóm trường và ngoại lệ.
7. Chuẩn bị/in văn bản với máy in, số bản, trang được chọn. Ghi job thật nếu có; phân biệt gửi lệnh, job kết thúc và cán bộ kiểm tra đủ bản giấy. Không tự in lại sau timeout chưa rõ kết quả.
8. Xuất gói đã kiểm tra gồm bản gốc, metadata xác nhận, text và vị trí nguồn, mã hồ sơ cùng manifest. Ghi tệp tạm, hoàn tất/kiểm tra rồi đổi tên. Cán bộ thực hiện chuyển USB và giấy.

Không cần tự mở mọi đính kèm bằng ứng dụng ngoài để lấy text; parser chạy trực tiếp giúp giảm thao tác và đổi focus. Mở trình xem khi đối chiếu/in hoặc khi parser không hỗ trợ, theo luồng xử lý tệp của đơn vị.

### Máy 2 — đăng ký, tham mưu và cập nhật chỉ đạo

1. Kiểm tra phương tiện/gói theo quy trình; nhập vào staging, xác minh version/bytes/hash/schema/các liên kết. Kiểm tra mã độc là công việc riêng, không thay bằng checkbox hoặc SHA256.
2. Chống lặp theo nguồn và phiên bản. Giữ kết quả máy 2 là nguồn thẩm quyền cho đăng ký/chỉ đạo; gói cũ hoặc sửa từ máy 1 không ghi đè tiến trình máy 2.
3. Tra cứu QLVB trước tạo mới bằng ID nguồn hoặc bộ khóa nghiệp vụ đã thống nhất. Phân biệt hồ sơ đã tồn tại, có khả năng trùng và cần tạo mới.
4. Điền trường, chọn danh mục, gắn tệp; đọc lại giá trị thực. Giai đoạn đầu dừng để cán bộ kiểm tra/Lưu, sau đó lấy mã/số đến và đối soát trên QLVB. Tự bấm Lưu chỉ là bước mở rộng sau khi kiểm chứng adapter và phạm vi thao tác đã được đơn vị xác định.
5. Gợi ý đề xuất dựa chức năng nhiệm vụ, phân công lãnh đạo, quy tắc định tuyến và hồ sơ mẫu. Cán bộ chỉnh, tạo phiếu từ mẫu chính thức, kiểm tra/in, trình kèm văn bản giấy.
6. Khi nhận phiếu đã duyệt, ghi chỉ đạo thực tế: lãnh đạo, nội dung, chủ trì/phối hợp, hạn, ngày quyết định và căn cứ. Có thể nhận scan để hỗ trợ đọc; chữ viết tay/chữ ký phải đối chiếu, không coi OCR là quyết định đã xác nhận.
7. Connector mở **đúng hồ sơ đã đăng ký**, cập nhật chỉ đạo đã xác nhận, lưu và đọc lại kết quả; ghi lần cập nhật và trạng thái đối soát. Chỉ cập nhật các trường chỉ đạo liên quan, không ghi đè toàn bộ biểu mẫu bằng snapshot cũ.
8. Giao giấy, ký nhận, nhiệm vụ, tiến độ và kết quả hoàn thành là các sự kiện riêng. Một nhiệm vụ xong không tự hoàn thành mọi việc trong hồ sơ.

Đề xuất thêm trạng thái theo dõi cập nhật đích: `NotStarted`, `Prepared`, `Dispatched`, `Verified`, `NeedsReconciliation`, `Failed`. Giữ riêng với trạng thái nghiệp vụ `Submitted/Approved/Delivered/...`; không trộn “đã duyệt” với “đã cập nhật QLVB”. Thứ tự giao giấy so với cập nhật đích theo quy trình đơn vị được xác định khi chốt flow.

## 6. Automation, OCR và AI: ranh giới trách nhiệm

| Công việc | Giải pháp ưu tiên | Vai trò AI |
|---|---|---|
| Tải danh sách, đính kèm, điền biểu mẫu | API/chức năng nhập tệp chính thức nếu có; nếu không dùng Playwright .NET cho web | Không cần AI cho các thao tác xác định |
| Hộp thoại Windows, ứng dụng đọc/in | Windows API/UI Automation đã khảo sát | Computer Use là dự phòng được giới hạn phạm vi |
| PDF/DOCX/TXT | Parser + renderer/OCR cục bộ | LLM không thay parser hoặc xác nhận đọc đủ trang |
| Số/ngày/đơn vị/người ký | Quy tắc bố cục, pattern, danh mục và kiểm tra kiểu dữ liệu | Hỗ trợ khi quy tắc chưa đủ; trả đề xuất có nguồn |
| Tham mưu xử lý | Quy tắc nghiệp vụ và tra cứu danh mục có hiệu lực | Tóm tắt, diễn đạt dự thảo, giải thích gợi ý dựa nguồn |
| Phê duyệt và cập nhật sau ký | Cán bộ xác nhận quyết định; connector thực thi dữ liệu đã chốt | Không tự thay đổi chỉ đạo/phê duyệt |
| Tổng hợp số liệu | Thuật toán xác định, invariant và fixtures đáp án | Có thể diễn đạt kết quả đã kiểm chứng |

Dùng Playwright .NET là lựa chọn đề xuất cho website hiện đại, cần khảo sát HTML/iframe/control/trình duyệt và OS trước khi khóa công cụ. Không dùng tọa độ làm mặc định. Các locator cần rõ nhãn/role/ID, kiểm tra duy nhất và hậu điều kiện; hết phiên hoặc lỗi giao diện thì dừng có giải thích.

Tài liệu SRS NFR-01 hiện ghi không HTTP ngoài loopback do SACD phát sinh. Cần cập nhật đặc tả để phản ánh yêu cầu mới đã xác nhận: **cho phép connector truy cập đúng website nội bộ của máy đó; cấm Internet và cấm cầu nối giữa hai mạng**. Không cần dựng backend SACD mạng. Bộ tải/cài dependencies được chuẩn bị bên ngoài và chuyển vào theo quy trình, không restore NuGet hoặc tự tải model lúc vận hành.

AI local nên nhận text đã trích xuất và tập nguồn nghiệp vụ liên quan; trả JSON có kiểm tra schema, gồm tóm tắt, đơn vị đề xuất, công việc, hạn/căn cứ và evidence IDs. Không coi điểm tự khai của model là xác suất đúng. Đo tỷ lệ trường đúng và mức sửa của cán bộ trên dữ liệu gán nhãn.

Kho tri thức ban đầu gồm danh mục đơn vị/chức năng, phân công lãnh đạo có hiệu lực, quy tắc định tuyến và phiếu mẫu đã xử lý. Bắt đầu bằng SQLite/FTS + bộ lọc và quy tắc; chưa cần thêm vector database. Không tự đưa mọi quyết định lịch sử thành quy tắc hiện hành. Thiếu căn cứ hoặc văn bản nhiều lĩnh vực thì đề nghị cán bộ xem xét thay vì đoán.

**Computer Use không phải điều kiện để AI tham mưu.** Với màn hình ổn định, connector xác định dễ kiểm chứng hơn agent nhìn ảnh và quyết định click liên tục. Nếu cần Computer Use về sau, triển khai adapter riêng: danh sách thao tác giới hạn, kiểm tra đúng đích, timeout và dừng, không chạy shell tùy ý, không trao quyền duyệt. Nội dung công văn là dữ liệu, không phải chỉ dẫn thay đổi quy tắc vận hành của agent. Đây là nhánh mở rộng vì BR-40 hiện thiết kế AI không có tool/shell; tách khỏi AI soạn đề xuất để không mở quyền mặc định.

## 7. Cấu trúc phần mềm và dữ liệu đề xuất

Giữ modular monolith. Tách theo use case trong Application, adapter trong Infrastructure; ViewModel chỉ điều khiển hiển thị. Không viết SQL, khởi tạo service hoặc logic connector trực tiếp trong ViewModel như một số đường gọi hiện có.

Các interface dưới đây là **đề xuất mới**, chưa có sẵn đầy đủ trong dự án:

- `IIntakePortalConnector`: liệt kê, đọc chi tiết, tải tệp; không đánh dấu đã tiếp nhận bên ngoài nếu chưa có hành động rõ.
- `IDocumentInspector` / `IPdfTextExtractor` / `IPdfRenderer` / `IOcrEngine`: kiểm tra và xử lý theo trang, giữ locator.
- `IMetadataExtractor`: kết quả typed, provenance, lý do Unknown/NeedsReview; không tự Confirm.
- `IQlvbConnector`: Find/Register/Reconcile/UpdateDecision; mỗi mutation có attempt và dữ liệu đầu vào đã khóa.
- `IPrintAdapter`, `ITransferPackageService`, `IWorkerSupervisor`, `IJobQueue`.
- `IProposalAdvisor`: rules trước, AI tùy chọn qua `IAiAssistant`; service kiểm tra evidence và dữ liệu trả về.

Case là aggregate nghiệp vụ điều phối Document, metadata version, proposal/decision version, task và attempt. Mở rộng quan hệ nhiều tài liệu trong case bằng `CaseDocument` có vai trò main/annex. Lần tiếp nhận riêng lưu ID nguồn và danh sách tệp; FileObject vẫn khử trùng bytes. Giữ định danh nguồn giữa hai kho, không dùng GUID cục bộ mới thay cho toàn bộ nguồn gốc.

Giữ riêng bản đề xuất với quyết định. Lưu nguồn hạn, ngày nhận/ngày ban hành/ngày ký/ngày đến và mã/số đến theo trường khác nhau. Danh mục dùng ID/version, không chỉ text tự do. Chưa rõ trường bắt buộc phải khảo sát biểu mẫu thật; không tự thêm ngày/hạn.

Chuẩn hóa gói USB phiên bản tiếp theo có package ID, origin workspace/case/version, schema/package version, danh sách tệp và hash/size, metadata/text/locator phiên bản. Không đưa code, model, session đăng nhập hoặc mật khẩu vào gói nghiệp vụ. Gói cài/cập nhật công cụ là loại gói khác. Hash bảo đảm đối chiếu toàn vẹn, không xác thực người gửi; ký/mã hóa nếu quy trình đơn vị yêu cầu, dùng cơ chế được đơn vị lựa chọn.

Migration thực hiện trên kho sao chép: backup → kiểm tra → chuyển đổi → đối chiếu counts/refs/hash → smoke → nâng version. Không sửa trực tiếp JSON/SQLite nghiệp vụ để vượt trạng thái; không xóa bảng cũ ngay khi chưa đối chiếu dữ liệu.

## 8. Phân bổ tài nguyên và nền tảng

Khởi điểm: một OCR/render/AI job nặng tại một thời điểm, xử lý từng trang, trả tiến độ, checkpoint và giải phóng ảnh; một phiên automation tương tác tại một thời điểm. Chỉ bật AI khi người dùng yêu cầu gợi ý hoặc quy tắc không đủ; không load model khi khởi động ứng dụng mặc định.

PdfPig là ứng viên lấy text/vị trí PDF; renderer và OCR phải là adapter riêng. Tesseract có tiếng Việt `vie`; thử cùng PaddleOCR nếu corpus cho thấy cần so sánh. Quyết định dựa độ đúng trường quan trọng, text đủ trang, RAM và p95, không chỉ điểm OCR chung. Khóa phiên bản và kiểm tra dependency/license của gói đã chọn.

Cho AI, thử model văn bản GGUF lượng tử hóa nhỏ qua llama.cpp hoặc runtime cục bộ tương đương, giới hạn ngữ cảnh và độ dài đầu ra. Qwen3-1.7B Q4_K_M được tài liệu gốc nêu như ứng viên; giữ như ứng viên benchmark, không khẳng định tốt nhất hoặc đủ tham mưu. Chạy CPU có thể chậm; không suy RAM từ dung lượng tệp model. Nếu không đạt, giữ rules/manual và đánh giá nâng RAM/thiết bị riêng trong chính môi trường được phép. Không giả định có máy mạnh dùng chung xuyên hai mạng.

Vào ngày đánh giá, .NET 8 dự kiến hết hỗ trợ **10/11/2026**. Đề xuất lập nhánh thử nâng lên .NET 10 LTS trước phát hành dài hạn, kiểm tra OS/WPF/UIA/SQLite/OpenXML và toàn bộ gói offline; giữ bản build hiện tại để hồi quy. Không chỉ đổi TargetFramework rồi coi hoàn thành.

## 9. Backlog có thứ tự và điều kiện hoàn thành

| Đợt | Công việc và đầu ra | Điều kiện ra khỏi đợt |
|---|---|---|
| A — xác lập nền | Chốt ma trận môi trường và trường dữ liệu; phân biệt demo/thật; sửa nhánh success giả, quyền, restore; đồng bộ chỉ mục/tình trạng tài liệu; dựng build/test tái lập và kế hoạch runtime | Không công bố thành công khi chưa có kết quả; thử quyền/khôi phục thực; build/xUnit/smoke mới có log gắn commit; phạm vi thí điểm ghi rõ |
| B — tiếp nhận đến USB | Parser/OCR, đối chiếu trường có nguồn, nhiều tệp/case, hàng đợi/checkpoint, in có attempt, gói chuyển phiên bản | TXT/DOCX/PDF text/scan/hỗn hợp đi qua đúng, không mất phụ lục; ngắt rồi tiếp tục không mất nguồn; máy 2 nhận metadata không nhập lại |
| C — QLVB và chỉ đạo | Khảo sát/xây connector hai web; đăng ký/tra trùng; phiếu mẫu thật; cập nhật sau duyệt, sửa sai/trả lại/gia hạn | UAT trên hệ thống thử thật; đúng hồ sơ/trường/tệp; save timeout được đối soát, không tạo lặp; quyết định khác đề xuất vẫn cập nhật đúng |
| D — vận hành và tổng hợp | Phân trang/tìm hiện hành, lịch sử thống nhất, snapshot/restore drill, sửa engine số liệu và mẫu báo cáo nếu đưa vào phát hành | Hoạt động offline trên máy sạch; đáp án số liệu đúng fixtures; báo cáo truy nguồn và đúng mẫu; không nhầm số đếm phiên UI với kho |
| E — AI local tùy chọn | Rules/danh mục trước, benchmark model, JSON/evidence validation và đánh giá cán bộ | Chứng minh lợi ích thời gian/chất lượng so với rules; tắt AI vẫn chạy mọi luồng lõi; không đưa Computer Use vào mặc định |

Không đưa thời hạn hoàn thành chắc chắn khi chưa có hai website/mẫu/corpus và quyền cài đặt. Có thể làm A, parser, schema, UI đối chiếu và fake portal trong khi chờ khảo sát; khả năng tương thích thực và năng suất phải đo tại đợt C. Tính năng tổng hợp rộng của đề tài vẫn giữ trong backlog; phần chưa sửa không được đưa vào bản nghiệp vụ chỉ để đủ danh sách chức năng.

## 10. Kế hoạch kiểm chứng và tiêu chí lựa chọn

Bộ mẫu đề xuất ban đầu 100–200 hồ sơ được phép sử dụng, phân tầng theo định dạng, chất lượng scan, số trang, nhiều phụ lục, văn bản tương tự và lĩnh vực. Chọn tập giữ lại để nghiệm thu, không chỉ dùng chính mẫu để tinh chỉnh quy tắc. Đây là quy mô thử đề xuất, không phải ngưỡng đã được đơn vị chốt.

| Nhóm kiểm tra | Trường hợp cần có | Bằng chứng cần lưu |
|---|---|---|
| Trích xuất | Số ở đầu trang và số được viện dẫn, ngày ký/ngày nhận khác nhau, tên người ký trong bảng, PDF text lỗi font, trang hỗn hợp | Đáp án từng trường, vị trí nguồn, trang thiếu/lỗi, tỷ lệ đúng và tỷ lệ cán bộ sửa |
| USB | Lặp gói, cùng nguồn khác version/hash, thiếu phụ lục, gói dở, schema lạ, vượt giới hạn thực khi stream | Kết quả mỗi gói, counts/refs/hash, xác nhận không ghi đè hồ sơ máy 2 |
| Web | Hết phiên, DOM đổi, iframe, combobox/ngày, mất mạng nội bộ sau save, mã đích đã tồn tại, lãnh đạo sửa người/hạn | Attempt, mã hồ sơ đích, giá trị đọc lại, trạng thái NeedsReconciliation và cách xử lý |
| In | Mất máy in, hết giấy, lỗi sau dispatch, in một phần, người dùng chọn lại số bản | Job/attempt và xác nhận giấy; không tự in lặp khi chưa rõ kết quả |
| Dữ liệu | Sửa revision đã tham chiếu, stale, mở lại sau mất điện, nhiều nhiệm vụ/case, thay đổi chỉ đạo | Invariant và lịch sử; chưa hoàn thành toàn case khi còn việc |
| Báo cáo/số liệu | Stock đảo thứ tự, ratio thập phân, dedup, kỳ/scope/metric khác, cumulative chồng, không evidence, claim chưa checked | Oracle và contribution; service chặn bản trình chưa đủ điều kiện |
| Offline/vận hành | Máy sạch không SDK/Internet; website nội bộ vẫn hoạt động; khôi phục sang kho mới; thử tài khoản khác | Build/tool/model hashes, runtime/OS, log mới, kết quả mở nguồn và hồ sơ sau restore |
| Tài nguyên/năng suất | Giờ cao điểm, file dài, kho tăng; OCR và AI tuần tự; cùng thao tác không có AI làm đối chứng | RAM tổng theo thời gian, p50/p95, lỗi, thời gian thao tác chủ động của cán bộ và backlog cuối ngày |

Các số NFR-05/06/09 và ngưỡng sắp hạn trong SRS đang là đề xuất hoặc cấu hình prototype. Giữ để làm mốc thảo luận, không đổi thành “đã đạt”. Đo CER OCR và độ đúng từng trường riêng; text đọc tốt chưa bảo đảm tìm đúng hạn/người ký. Lưu commit/build/config/corpus và công cụ cùng từng kết quả.

## 11. Đầu vào cần thu thập để hiện thực connector

- Hai website: tên/phiên bản, URL nội bộ của từng máy, trình duyệt/OS; cấu trúc danh sách, chi tiết, tải tệp, đăng ký, cập nhật chỉ đạo và tra cứu.
- Phương thức đăng nhập, hết phiên, CAPTCHA/MFA nếu có; tài khoản/môi trường thử và phạm vi thao tác automation được phép. Không yêu cầu đưa mật khẩu vào mã/mapping.
- Danh sách trường bắt buộc, danh mục và quy tắc trùng; API/import nếu có; cách xác nhận save thành công và lấy mã hồ sơ đích.
- Mẫu công văn đa dạng và phiếu chính thức, danh mục đơn vị/lãnh đạo có hiệu lực, quy tắc phân công và hạn.
- Khối lượng/ngày, số trang/tệp, tệp lớn nhất, tỉ lệ scan, máy in/driver và cách xác nhận giấy; loại USB và cách kiểm tra phương tiện/gói.
- Chính sách quyền thư mục, giữ bản gốc/backup/đổi máy, quyền cài runtime/công cụ và cách cập nhật offline.

Đây là các đầu vào cho triển khai thật; không cản việc chuẩn hóa code, parser, data contracts và kiểm thử giả lập trong phạm vi hiện có.

## 12. Tài liệu kỹ thuật đối chiếu

- [Playwright .NET — Installation](https://playwright.dev/dotnet/docs/intro): hỗ trợ .NET và yêu cầu môi trường; [Auto-waiting](https://playwright.dev/docs/actionability): điều kiện trước thao tác. Không phải bằng chứng tương thích website đơn vị.
- [PdfPig](https://github.com/UglyToad/PdfPig): đọc và trích xuất PDF; thứ tự đọc cần xử lý theo bố cục, không mặc định mọi PDF có text hữu ích.
- [Tesseract — dữ liệu ngôn ngữ](https://tesseract-ocr.github.io/tessdoc/Data-Files-in-different-versions.html): có mô hình tiếng Việt `vie`; chất lượng phải đo theo corpus.
- [llama.cpp](https://github.com/ggml-org/llama.cpp): runtime inference cục bộ với các mức lượng tử hóa; không chứng minh tốc độ trên máy đích.
- [.NET 8/9 end of support](https://devblogs.microsoft.com/dotnet/dotnet-8-9-end-of-support/) và [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy): .NET 8 hết hỗ trợ 10/11/2026; .NET 10 LTS đến 14/11/2028.

Đề xuất phát hành trước luồng **tiếp nhận → đọc/OCR → đối chiếu → in → USB → QLVB → phiếu → cập nhật chỉ đạo**, với dữ liệu và trạng thái kiểm chứng được. AI tham mưu là nhánh tăng năng suất sau đó; Computer Use chỉ chọn khi khảo sát cho thấy connector xác định không xử lý được phần giao diện cần thiết.
