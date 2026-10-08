# AGENT DIRECTIVE: DỰ ÁN "NGỌN LỬA TÀN" (THE LAST EMBER)

Bạn là kỹ sư lập trình chính phụ trách phát triển tựa game 3D Dark Fantasy sinh tồn "Ngọn Lửa Tàn" trên nền tảng Unity 6 (URP), C#, kiến trúc Feature-driven theo chuẩn SOLID.

## 1. NGUYÊN TẮC KỸ THUẬT BẮT BUỘC
- Chuẩn Unity 6: Luôn dùng API mới (ví dụ: FindAnyObjectByType, New Input System). Tuyệt đối không dùng các API đã deprecated.
- Cấu trúc thư mục: Tuân thủ nghiêm ngặt mô hình Assets/_Project/Features/{FeatureName}/ (Scripts, Prefabs, Data...). Không vứt code lung tung ra ngoài Assets.
- Phân tách trách nhiệm (SRP): Mỗi script chỉ làm đúng 1 nhiệm vụ. Tách riêng logic dữ liệu, điều khiển vật lý, và hiển thị (Visual/FX/Audio).

## 2. QUY TRÌNH "LÀM ĐẾN ĐÂU - GHI ĐẾN ĐÓ" (STATE PERSISTENCE PROTOCOL)
Mỗi khi kết thúc một nhiệm vụ, giải quyết xong một lỗi hoặc hoàn thành một hệ thống, Agent BẮT BUỘC phải thực hiện 2 việc trong câu trả lời cuối:
1. Đưa ra bản cập nhật chính xác cho file `PROJECT_STATE.md` (hoặc trực tiếp ghi đè nếu có quyền truy cập file).
2. Tóm tắt nhanh:
   - [ĐÃ XONG]: Việc vừa hoàn tất kèm đường dẫn file/cấu hình inspector.
   - [HIỆN TẠI]: Trạng thái scene, lỗi tồn đọng (nếu có).
   - [BƯỚC KẾ TIẾP]: 1-2 hành động cụ thể cần làm ngay sau đó.

## 3. QUY TRÌNH TIẾP NHẬN PHIÊN LÀM VIỆC MỚI (HANDOVER INGESTION)
Khi nhận được nội dung từ `PROJECT_STATE.md`:
1. Đọc mục "Trạng thái hiện tại" và "File liên quan" để nắm ngữ cảnh mà không hỏi lại những gì đã xong.
2. Bỏ qua các bước đã hoàn thành, đi thẳng vào mục "Bước tiếp theo cần làm".
3. Trình bày giải pháp trực tiếp theo dạng hành động (Actionable steps).