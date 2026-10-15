# ĐĂNG KÝ GIẢNG VIÊN HƯỚNG DẪN VÀ ĐỀ TÀI LÀM ĐỒ ÁN TỐT NGHIỆP

**Đề tài:** SACD — Software for Automatically Compiling Documents  
**Phiên bản tài liệu:** 1.0 — Dự thảo đăng ký  
**Ngày biên soạn:** 15/10/2026  
**Người đăng ký:** Đàm Quang Trung  
**Thời gian làm đồ án:** từ **[chưa xác nhận]** đến **[chưa xác nhận]**.

## 1. Thông tin về người hướng dẫn

| Vai trò | Tên đầy đủ | Số điện thoại | Địa chỉ email | Chức danh |
|---|---|---|---|---|
| Người hướng dẫn | Chưa xác nhận cho đề tài SACD | Chưa xác nhận | Chưa xác nhận | Chưa xác nhận |

## 2. Nội dung của đồ án

### 2.1. Tên đồ án

- **Tên tiếng Anh:** Software for Automatically Compiling Documents.
- **Tên tiếng Việt:** Phần mềm hỗ trợ tự động hóa quy trình xử lý văn bản và tổng hợp tài liệu.
- **Viết tắt:** SACD.
- **Loại phần mềm:** ứng dụng desktop Windows, vận hành dữ liệu cục bộ, AI là mô-đun tùy chọn.

### 2.2. Nội dung chính đề xuất bao gồm kết quả và sản phẩm

#### Lý thuyết và thực hành dạng văn bản

**Giới thiệu tổng quan về hệ thống SACD**

SACD được đề xuất nhằm hỗ trợ cán bộ làm công tác văn thư, xử lý văn bản và tổng hợp tài liệu trong môi trường cơ quan có các máy tính tách biệt về mạng. Phần mềm tổ chức dữ liệu văn bản, hỗ trợ các thao tác lặp lại và tạo báo cáo có căn cứ nguồn; các quyết định nghiệp vụ vẫn do người có trách nhiệm thực hiện.

Quy trình cần hỗ trợ bắt đầu tại máy nhận email nội bộ: kiểm tra thư, đọc và phân loại văn bản, xác định thông tin và công việc liên quan, in tài liệu rồi chuẩn bị tệp để chuyển qua USB được phép. Sau khi chuyển vật lý sang máy quản lý văn bản, cán bộ kiểm tra thời hạn, nhập dữ liệu, đề xuất lãnh đạo hoặc đội xử lý, in phiếu trình trưởng phòng/lãnh đạo duyệt, cập nhật chỉ đạo, giao văn bản, ghi nhận ký nhận và tiếp tục cập nhật tình trạng xử lý. Dữ liệu từ nhiều nguồn được tra cứu, đối chiếu và tổng hợp thành báo cáo để trình lãnh đạo.

SACD được xây dựng dưới dạng ứng dụng desktop C#/.NET với WPF, không yêu cầu máy chủ web riêng hoặc truy cập Internet để vận hành nghiệp vụ chính. Có thể cài độc lập trên từng máy với hồ sơ chức năng phù hợp. Hai kho dữ liệu không tự kết nối qua mạng; trao đổi tệp phải đi qua quy trình USB và kiểm tra của đơn vị.

Máy cơ quan hiện chưa đáp ứng yêu cầu chạy mô hình ngôn ngữ, vì vậy AI mặc định tắt. Phần mềm vẫn phải nhập tài liệu, cho kiểm tra/sửa dữ liệu, quản lý thời hạn, ghi và phát lại thao tác đã cấu hình, lập phiếu, tổng hợp theo quy tắc, xuất Word và sao lưu khi chưa có AI. Việc bổ sung AI về sau đi qua điểm tích hợp đã định nghĩa, không thay đổi cách tính số liệu hoặc thẩm quyền phê duyệt.

**Thực trạng công việc**

- Cán bộ phải lặp lại việc đọc thư, lưu tệp, xem văn bản, xác định thông tin và nhập lại vào phần mềm khác.
- Bản giấy, bản scan và tệp điện tử tồn tại song song; kết quả OCR hoặc thông tin trích xuất có thể sai, cần đối chiếu.
- Hai môi trường máy tách biệt làm phát sinh bước chuẩn bị, kiểm tra và chuyển tệp thủ công qua USB.
- Thời hạn văn bản và nhiệm vụ dễ bị bỏ sót khi việc theo dõi nằm rải rác trong sổ, tệp và hệ thống khác nhau.
- Đề xuất xử lý, kết quả duyệt, giao văn bản và ký nhận phải được cập nhật qua nhiều lần thao tác.
- Tổng hợp báo cáo từ nhiều nguồn mất thời gian; có nguy cơ cộng trùng, hiểu sai kỳ hoặc mất dấu nguồn của nhận định và số liệu.
- Thao tác chuột/phím lặp lại có thể tự động hóa một phần, nhưng phải có điểm dừng cho người dùng và kiểm tra trạng thái giao diện.

**Giải pháp đề xuất**

1. Số hóa hồ sơ văn bản và dữ liệu xử lý theo từng máy, giữ bản gốc và lịch sử thay đổi.
2. Đọc text/OCR, kết hợp nhập tay và quy tắc nhận diện; dữ liệu chưa chắc chắn được đánh dấu để xác minh.
3. Gắn metadata, nhiệm vụ, thời hạn, nơi xử lý và thông tin giao nhận vào cùng hồ sơ; ghi căn cứ cho các thay đổi quan trọng.
4. Tạo gói tệp có bảng kê/hash để hỗ trợ chuyển qua USB được phép, vẫn giữ bước chuyển và kiểm tra vật lý.
5. Ghi và phát lại các thao tác đã chọn trên ứng dụng email hoặc phần mềm quản lý văn bản; ưu tiên nhận diện thành phần giao diện, dùng tọa độ khi cần.
6. Tạo phiếu đề xuất, ghi kết quả duyệt thực tế và theo dõi giao/nhận/xử lý; không tự phê duyệt hoặc ký thay.
7. Tổng hợp số liệu bằng quy tắc xác định, soạn báo cáo từ dữ liệu đã xác nhận và nguồn đã chọn, rà soát trước xuất Word.
8. Chuẩn bị giao diện tích hợp AI cục bộ để bổ sung khi máy đáp ứng; chưa bắt buộc cài model trong giai đoạn hiện tại.

Đối tượng sử dụng trực tiếp là cán bộ tiếp nhận/văn thư/cán bộ tổng hợp và người được giao quản lý cấu hình. Trưởng phòng/lãnh đạo, đội/cán bộ xử lý và người ký nhận là các chủ thể của quy trình nghiệp vụ; chưa mặc định cấp tài khoản trực tuyến cho các đối tượng này.

#### Chương trình phần mềm

**Ứng dụng desktop và giao diện người dùng**

- C# và .NET 10 LTS; giao diện WPF/XAML, tổ chức theo MVVM.
- Các màn hình: Tổng quan, Tiếp nhận, Tài liệu/OCR, Thông tin văn bản, Công việc và thời hạn, Đề xuất/duyệt/giao nhận, Kịch bản thao tác, Tìm kiếm, Số liệu, Mẫu/báo cáo, Cấu hình/nhật ký/sao lưu.
- Cho phép người dùng xem trước, xác nhận dữ liệu và chủ động bắt đầu/dừng thao tác tự động.

**Xử lý nghiệp vụ và dữ liệu**

- Kiến trúc phân lớp: Desktop, Application, Domain và Infrastructure.
- SQLite qua Microsoft.Data.Sqlite; FTS5 cho tìm kiếm toàn văn. Bản gốc lưu trong kho tệp cục bộ, CSDL lưu metadata, đường dẫn/hash, phiên bản và quan hệ nguồn.
- Open XML SDK cho DOCX và mẫu Word; PdfPig cho text PDF, Poppler cho tác vụ dựng trang theo bộ công cụ được chốt; Tesseract vie/eng cho OCR cục bộ.
- SHA256, phiên bản nguồn và kiểm tra toàn vẹn khi nhập/chuyển/backup; số liệu tính bằng decimal theo quy tắc được xác nhận.

**Ghi và phát lại thao tác máy**

- Microsoft UI Automation/FlaUI để tìm nút, ô nhập, cửa sổ và thực hiện hành động tương ứng.
- Win32 Windows Hooks (`SetWindowsHookEx`, `WH_MOUSE_LL`, `WH_KEYBOARD_LL`) để ghi sự kiện trong phiên ghi do người dùng bật.
- `GetCursorPos`, `WindowFromPoint`, `ScreenToClient` và thông tin cửa sổ để lưu vị trí cùng ngữ cảnh; `SendInput` làm phương án mô phỏng chuột/phím khi cần.
- Kịch bản JSON/SQLite chứa các bước, biến, selector, tọa độ dự phòng, điều kiện chờ, timeout, điểm người dùng can thiệp và kết quả chạy.
- Không ghi mật khẩu; không vượt quyền/UAC; không phát lại mù khi cửa sổ, phần tử hoặc điều kiện màn hình không còn khớp.

**AI tùy chọn trong tương lai**

- Định nghĩa `IAiAssistant` độc lập với nghiệp vụ lõi; trạng thái chưa cấu hình phải được xử lý bình thường.
- Ứng viên thử nghiệm: Qwen3-1.7B, GGUF Q4_K_M, chạy cục bộ bằng llama.cpp; chưa chốt model chính thức hoặc cam kết chất lượng/tốc độ.
- AI chỉ đề xuất phân loại, thông tin, nhiệm vụ, tóm tắt, hỏi đáp có nguồn và câu chữ; người dùng xác nhận trước sử dụng.
- Nếu worker AI dùng loopback trên cùng máy, đây là chi tiết tích hợp nội bộ; SACD vẫn là ứng dụng desktop, không cần máy chủ web nghiệp vụ riêng.

**Các sản phẩm khác**

- Phiếu đăng ký đề tài và Report 1 giới thiệu dự án.
- Tài liệu SRS, SDS, Use Case, mô hình dữ liệu/ERD và ma trận truy vết sẽ được cập nhật theo phạm vi này.
- Mã nguồn desktop, gói cài/chạy Windows, hướng dẫn sử dụng và quản trị công cụ offline.
- Danh mục cấu hình, mẫu phiếu/báo cáo đã được cung cấp, kịch bản RPA mẫu và dữ liệu giả để kiểm thử.
- Kế hoạch, ca kiểm thử và kết quả chạy có bằng chứng; không lấy số lượng ca hoặc bản EXE làm bằng chứng nghiệm thu.

## 3. Phạm vi của đề tài

### INSCOPE

Phạm vi dưới đây là mục tiêu phát triển và nghiệm thu của SACD; không phải danh sách chức năng đã hoàn thành trong bản thử nghiệm trước.

| Nhóm | Nội dung thuộc phạm vi | Mã tính năng |
|---|---|---|
| a. Kho, quyền và danh mục | Kho cục bộ, SID/NTFS, danh mục đơn vị/người xử lý và ánh xạ trường | FE-01, FE-21 |
| b. Tiếp nhận và số hóa | Email nội bộ theo cách được phép; nhập file/scan, đọc text, OCR, đối chiếu/sửa | FE-02–FE-04 |
| c. Thông tin và phân loại | Metadata, nhiệm vụ, nguồn chứng minh, chủ đề, mật/khẩn và mốc liên quan giải mật | FE-05, FE-06 |
| d. In và chuyển tệp | In văn bản/phiếu; đóng gói, kiểm tra và chuyển USB qua thao tác vật lý được phép | FE-07, FE-08 |
| e. Theo dõi xử lý | Cảnh báo hạn; hỗ trợ điền phần mềm quản lý văn bản; đề xuất, cập nhật duyệt, giao và ký nhận | FE-09–FE-13 |
| f. Tra cứu và tổng hợp | Lịch sử tài liệu, số liệu, chống trùng, mẫu phiếu/báo cáo và xuất Word có nguồn | FE-14–FE-16 |
| g. Ghi phát lại thao tác | Ghi chuột/phím có chủ đích, sửa kịch bản, ánh xạ biến, phát lại có điều kiện và điểm dừng | FE-17, FE-18 |
| h. Vận hành | Cấu hình, nhật ký metadata, sao lưu/kiểm tra phục hồi | FE-19 |
| i. Khả năng mở rộng AI | Điểm tích hợp tắt mặc định, không phụ thuộc model để vận hành lõi | FE-20 ở mức kiến trúc; model thực tế chưa bắt buộc |

Các chức năng tích hợp email, in, OCR và RPA cần được khảo sát, chọn công cụ và chạy trên ứng dụng/thiết bị đích trước khi xác nhận đạt. Chế độ thủ công được giữ để không làm gián đoạn nghiệp vụ khi tích hợp chưa sẵn sàng.

**Các nguyên tắc bắt buộc**

- Không tạo kết nối mạng trực tiếp giữa máy nhận thư và máy quản lý văn bản.
- Không đưa tài liệu lên cloud hoặc AI Internet trong luồng vận hành dự kiến.
- Việc đọc email nội bộ sử dụng môi trường và quyền đã được cho phép trên máy nhận thư.
- Không tự quyết định nơi xử lý cuối cùng, tự duyệt, tự giải mật hoặc ký nhận thay con người.
- Chỉ ghi thao tác trong phạm vi được người dùng chọn; có dừng khẩn cấp, timeout và kiểm tra đúng ứng dụng đích.
- Không coi UI Automation tương thích với mọi phần mềm; phải có kịch bản kiểm thử thực tế cho từng ứng dụng/phiên bản.
- Không coi xử lý hash, sao lưu hoặc kiểm tra đầu vào là chứng nhận đủ điều kiện xử lý bí mật nhà nước hay USB tuyệt đối sạch mã độc.

### OUTSCOPE

- Xây SACD thành web server–client, cloud hoặc dịch vụ công cộng; truy cập SACD từ xa qua Internet.
- Đồng bộ trực tiếp qua mạng giữa các máy đang cách ly; tự động bỏ bước kiểm tra/chuyển USB theo quy trình đơn vị.
- Thay thế hệ thống email hoặc phần mềm quản lý văn bản hiện có; can thiệp trực tiếp CSDL/API chưa được cho phép.
- Ghi thao tác ngầm toàn máy, thu thập mật khẩu, vượt UAC/quyền tài khoản, điều khiển ứng dụng ngoài phạm vi chọn.
- Phê duyệt, giải mật, ký số hoặc ký nhận tự động thay người có thẩm quyền.
- Huấn luyện mô hình nền tảng, dịch vụ AI Internet và yêu cầu bắt buộc chạy LLM trên máy cơ quan hiện tại.
- Ứng dụng Android/iOS riêng, quản lý lịch trực, đổi ca hoặc bàn giao ca trực của dự án DSMS.
- Bảo đảm điều khiển mọi phần mềm, mọi máy scan, mọi định dạng tài liệu hoặc nhận dạng chữ viết tay với độ đúng tuyệt đối.

## 4. Các nhận xét khác

SACD được phát triển theo các bước: khảo sát quy trình/ứng dụng đích; hoàn thiện lõi desktop; thêm kịch bản RPA và trao đổi tệp có kiểm soát; hoàn thiện tổng hợp/báo cáo; đánh giá và bổ sung AI khi có điều kiện. RPA thuộc mục tiêu chính của đề tài mới; chạy model AI thực tế là hướng mở rộng, không là điều kiện nghiệm thu lõi.

Cần chốt lịch đồ án, người hướng dẫn, cấu hình CPU/Windows/ổ đĩa, phần mềm email/QLVB đích, mẫu phiếu/báo cáo, danh mục nghiệp vụ và quy tắc hạn/số liệu trước khi đặt mốc nghiệm thu chi tiết. Chưa đặt chỉ tiêu tiết kiệm thời gian hoặc độ chính xác như kết quả đã đạt; phải đo trên dữ liệu thử được phép và có đáp án đối chiếu.

Việc triển khai thử nghiệm sử dụng dữ liệu giả hoặc dữ liệu được phép sử dụng; mọi áp dụng thực tế cần được đơn vị có trách nhiệm xem xét theo môi trường và quy trình liên quan.

| Người hướng dẫn | Người đăng ký đề tài |
|---|---|
| Ký và ghi rõ họ tên sau khi được xác nhận | Lào Cai, ngày …… tháng …… năm …… |
|  | Ký và ghi rõ họ tên |
|  | **Đàm Quang Trung** |

## Ghi chú nguồn và trạng thái bản dự thảo

Khung tài liệu chuyển từ mẫu `Funix-Phieu-dang-ky-de-tai_updated(1).docx`; nội dung nghiệp vụ DSMS, lịch tháng 5–8/2026 và người hướng dẫn của đề tài cũ không được coi là thông tin đã xác nhận cho SACD. Các yêu cầu SACD dựa trên trao đổi hiện tại và phần nội dung truy xuất được của [cuộc trò chuyện quy trình văn bản](https://chatgpt.com/c/6ab0a359-3c38-83ec-ae4c-a1d922b76445). Chưa có toàn bộ transcript để kiểm chứng tính đầy đủ tuyệt đối. Xem phụ lục A–D của Report 1 đi kèm để đối chiếu từng bước, điểm cần chốt và căn cứ kỹ thuật.
