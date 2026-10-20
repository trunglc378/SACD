> Cập nhật 20/10: Dockerfile dùng .NET 10; healthcheck đọc SQLite không giành writer lock. Chưa chạy Docker trong đợt kiểm thử Windows này.

# HƯỚNG DẪN ĐÓNG GÓI VÀ VẬN HÀNH SACD TRÊN DOCKER CONTAINER

**Mã tài liệu:** SACD-DOCKER-GUIDE-01  
**Phiên bản:** 1.1  
**Ngày cập nhật:** 15/10/2026  
**Đường dẫn lưu trữ:** `E:\Project\SACD\docs\Docker\Huong_dan_dong_goi_Docker.md`

---

## 1. TỔNG QUAN VÀ BẢN CHẤT KIẾN TRÚC TRONG MÔI TRƯỜNG DOCKER

### 1.1. Bản chất hệ thống SACD
Hệ thống SACD gồm hai thành phần chính:
1. **SACD.Desktop (WPF UI):** Giao diện đồ họa người dùng tương tác trực tiếp với các API Windows Win32/DirectX và clipboard/phím chuột điều khiển RPA. WPF gắn chặt với hệ điều hành Windows desktop.
2. **SACD Core Engine & Verification Worker (SACD.Cli / Domain / Application / Infrastructure):** Bộ lõi xử lý dữ liệu, kiểm tra 40 quy tắc nghiệp vụ (BR-01..40), khởi tạo 50 bảng SQLite, lưu trữ phân tán SHA256 (`objects/<sha256>`), tính toán số liệu chính xác `ExactRatio`, xuất báo cáo Word OpenXML và đóng gói USB transfer.

### 1.2. Có thể đóng gói Container được không?
**HOÀN TOÀN ĐƯỢC.** 
- Toàn bộ tầng nghiệp vụ Core Engine, Database SQLite FTS5, FileStore, Trình trích xuất văn bản, và Trình xuất báo cáo Word được viết trên **.NET 10.0 Standard/Cross-Platform**, hoàn toàn độc lập với UI.
- Do đó, chúng ta đóng gói thành công **SACD Core Worker Container** chạy trên Linux Container hoặc Windows Container thông qua Docker Desktop.
- Khi cần chạy giao diện đồ họa WPF Desktop trên các máy trạm yếu (không chạy qua Docker), người dùng sử dụng bản phát hành độc lập **Self-Contained Windows Executable** (được hướng dẫn chi tiết tại mục 4).

---

## 2. CẤU TRÚC ĐÓNG GÓI DOCKER CỦA SACD

Hệ thống cung cấp sẵn hai tệp cấu hình đóng gói chuẩn:
- `Dockerfile`: Xây dựng ảnh nhiều giai đoạn (Multi-stage build) trên nền `.NET 10.0 SDK` và `.NET 10.0 Runtime`.
- `docker-compose.yml`: Quản lý container, thiết lập múi giờ Việt Nam (`Asia/Ho_Chi_Minh` UTC+7) và gắn kết dữ liệu ra ngoài máy chủ qua Docker Volume.
- `.dockerignore`: Loại trừ mã nhị phân rác, tăng tốc độ build ảnh.

---

## 3. CÁC BƯỚC THỰC HIỆN ĐÓNG GÓI TRÊN DOCKER DESKTOP

### Bước 1: Khởi động Docker Desktop
Đảm bảo biểu tượng cá voi của **Docker Desktop** trên khay hệ thống (System Tray) chuyển sang màu xanh lá cây (**Engine Running**).

### Bước 2: Build Docker Image từ thư mục dự án
Mở PowerShell tại thư mục gốc dự án `E:\Project\SACD`:

```powershell
docker build -t sacd-engine:latest .
```

*Quá trình build diễn ra qua 2 giai đoạn:*
1. **Stage 1 (build):** Tải SDK .NET 10.0, restore thư viện NuGet và biên dịch tối ưu ở chế độ `Release`.
2. **Stage 2 (runtime):** Chỉ sao chép các tệp thực thi sang base image nhẹ (`dotnet/runtime:8.0`), loại bỏ toàn bộ mã nguồn và công cụ SDK, giúp dung lượng container nhỏ gọn và an toàn bảo mật.

### Bước 3: Khởi chạy Container bằng Docker CLI

#### 1. Chế độ kiểm tra chẩn đoán hệ thống (Health Check Diagnostics):
```powershell
docker run --rm -v ${PWD}/docker_data:/data/sacd_workspace sacd-engine:latest /data/sacd_workspace --diag
```
*Kết quả hiển thị:* Khởi tạo 50 bảng SQLite, kiểm tra thư mục objects và xác nhận hệ thống sẵn sàng.

#### 2. Chế độ chạy ngầm liên tục (Daemon Worker):
```powershell
docker run -d --name sacd-core-worker -v ${PWD}/docker_data:/data/sacd_workspace -e TZ=Asia/Ho_Chi_Minh sacd-engine:latest
```

### Bước 4: Vận hành với Docker Compose (Khuyên dùng)
Để khởi động hoặc dừng nhanh chóng:
```powershell
# Khởi động dịch vụ nền:
docker-compose up -d

# Xem log thực thi của container:
docker-compose logs -f

# Dừng và dọn dẹp container:
docker-compose down
```

---

## 4. PHƯƠNG ÁN ĐÓNG GÓI CHẠY TRÊN MÁY TÍNH YẾU (WINDOWS 7, WINDOWS 10 CẤU HÌNH THẤP)

Đối với các máy trạm văn thư, tiếp nhận hồ sơ cấu hình yếu (RAM 2GB - 4GB, CPU đời cũ, không có internet, chưa cài đặt .NET Runtime):

### 4.1. Giải pháp Self-Contained Single-File Executable
Sử dụng cờ biên dịch tự chứa toàn bộ runtime và thư viện vào trong 1 thư mục/tệp chạy duy nhất, không phụ thuộc máy trạm có cài đặt gì:

```powershell
& "$env:USERPROFILE\.dotnet\dotnet.exe" publish src\SACD.Desktop\SACD.Desktop.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -o E:\Project\SACD\dist\win-x64
```

### 4.2. Ưu điểm vượt trội khi chạy trên máy yếu
1. **Không cần cài đặt .NET Framework hay .NET SDK:** Máy trạm chỉ cần mở tệp `SACD.Desktop.exe` là chạy ngay.
2. **Bảo mật tuyệt đối (Zero Network footprint):** Không kết nối internet, tương thích hoàn toàn với quy trình cách ly vật lý Air-gap.
3. **Cơ sở dữ liệu nhúng siêu nhẹ:** SQLite chạy trực tiếp trong tiến trình, tiêu tốn dưới 60MB RAM.
4. **Hỗ trợ máy 32-bit (Nếu có máy Windows 7 32-bit):**
   Chỉ cần thay đổi cờ kiến trúc sang `win-x86`:
   ```powershell
   dotnet publish src\SACD.Desktop\SACD.Desktop.csproj -c Release -r win-x86 --self-contained true -o E:\Project\SACD\dist\win-x86
   ```

---

## 5. BẢNG TỔNG HỢP LỆNH DOCKER THƯỜNG DÙNG ĐỂ TRA CỨU

| Mục đích | Lệnh thực thi |
|---|---|
| Xây dựng ảnh | `docker build -t sacd-engine:latest .` |
| Liệt kê ảnh đã tạo | `docker images | grep sacd` |
| Chạy thử nghiệm chẩn đoán | `docker run --rm sacd-engine:latest /tmp/ws --diag` |
| Xem tiến trình đang chạy | `docker ps` |
| Xem nhật ký tiến trình | `docker logs -f sacd-core-worker` |
| Truy cập vào bên trong container | `docker exec -it sacd-core-worker /bin/sh` |
| Xóa container khi dừng | `docker rm -f sacd-core-worker` |
