# Cài đặt và sử dụng — bản sửa 20/10/2026

Xem [README](../../README.md) và [bản sửa/chạy thử](../09_SACD_Ban_sua_va_chay_thu_2026-10-20.md). Dùng .NET 10; bản self-contained không cần SDK. Không chạy lệnh tải Internet trên máy nghiệp vụ cách ly.

1. Mở SACD.Desktop.exe từ toàn bộ thư mục publish. Chọn vai trò Máy 1 — Tiếp nhận hoặc Máy 2 — Quản lý và tạo kho riêng, ngoài thư mục chương trình, đúng tài khoản Windows.
2. Mục Quy trình văn thư: tiếp nhận tệp thật, kiểm tra gợi ý và bản gốc. Ngày yyyy-MM-dd; hạn chưa rõ để trống. Trang scan cần OCR, không thể xác nhận toàn văn.
3. In trong ứng dụng đọc, kiểm tra giấy rồi xác nhận in; xuất ZIP, kiểm tra và chuyển USB vật lý.
4. Máy 2 nhập gói; nhập QLVB thủ công hoặc UIA với mapping đã khảo sát. Kiểm tra/Lưu trên QLVB rồi nhập mã hồ sơ đích và đối soát.
5. Tạo phiếu Word/QR tham khảo; đối chiếu mẫu đơn vị, in/trình. Nhận phiếu duyệt rồi ghi người duyệt/người được giao/hạn/căn cứ.
6. Cập nhật chỉ đạo trên đúng hồ sơ QLVB. Nhập căn cứ rồi bấm **Đối soát chỉ đạo đã cập nhật QLVB**, sau đó ghi nhận giao giấy/ký nhận/hoàn thành riêng.

Đối soát chỉ đạo là xác nhận thủ công của cán bộ, chưa kiểm tra website tự động. QR chỉ chứa mã hồ sơ; các tab mẫu/mô phỏng không thao tác QLVB thật.

Backup snapshot ở ngoài kho. Service phục hồi chỉ nhận thư mục mới, xác minh SID/schema/SQLite/FK/object. Chưa có UI phục hồi; không giải nén đè kho đang mở. Cần diễn tập và công cụ vận hành trước bàn giao.
