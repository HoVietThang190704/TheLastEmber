# TRẠNG THÁI DỰ ÁN: NGỌN LỬA TÀN (THE LAST EMBER)
Cập nhật lần cuối: 2026-10-08 | Engine: Unity 6 (6000.0.41f1) URP | Input: Both (New + Legacy)

---

## 1. TIẾN ĐỘ TỔNG THỂ
- [x] Khởi tạo cấu trúc thư mục chuẩn Feature-driven (`Assets/_Project/...`)
- [x] Dựng Graybox cơ bản (`Ground`, `Player` Capsule, `Main Camera`)
- [x] Di chuyển nhân vật 8 hướng mượt mà (`PlayerMovement.cs`)
- [x] Camera Isometric góc nhìn 50 độ bám theo nhân vật (`IsometricCameraFollow.cs`)
- [x] Đại Hỏa Lò (Great Furnace): Cylinder model + Point Light bán kính co giãn (`GreatFurnace.cs`)
- [x] Cảnh báo vùng an toàn sương mù (`MistZoneDetector.cs`)
- [x] Chu kỳ Ngày/Đêm: Đổi màu trời, xoay Sun Light, co giãn bán kính Hỏa Lò (`DayNightManager.cs`)
- [x] Economy MVP: ví tài nguyên (`Wood`, `Stone`, `SilverEmber`), vật phẩm nhặt được, nạp củi kéo dài năng lượng Hỏa Lò
- [ ] [ĐANG LÀM]: Kết nối Economy MVP vào scene bằng prefab/inspector (`ResourceWallet`, `FurnaceFuelInteractor`, `CollectibleResource`)
- [ ] [CHƯA LÀM]: Hệ thống Xây dựng theo lưới (Building Grid System)
- [ ] [CHƯA LÀM]: Máy trạng thái chiến đấu (Combat FSM - Tấn công, Thể lực)
- [ ] [CHƯA LÀM]: Hệ thống Chợ Đêm & Đợt quái sương mù (Night Wave Spawner)

---

## 2. CHI TIẾT CÁC COMPONENT ĐANG HOẠT ĐỘNG TRONG SCENE
| Đối tượng (Hierarchy) | Script / Component đính kèm | Tham chiếu (References) đã nối | Ghi chú trạng thái |
| :--- | :--- | :--- | :--- |
| **Ground** | Mesh Renderer, Box/Mesh Collider | N/A | Scale: (5, 1, 5) |
| **Player** | `CharacterController`, `PlayerMovement`, `MistZoneDetector` | `MistZoneDetector.furnace` -> `GreatFurnace` | Di chuyển WASD mượt mà, nhận diện ra/vào sương |
| **Main Camera** | `Camera`, `IsometricCameraFollow` | `Target` -> `Player` | Offset: (0, 12, -8), SmoothSpeed: 5 |
| **GreatFurnace** | `GreatFurnace` | `furnaceLight` -> con `FurnaceLight` | Level 1, BaseRadius = 15m, Gizmos vàng hiển thị tốt |
| ↳ **FurnaceLight** | `Light` (Point Light) | Màu cam vàng, Intensity: 15, Range: 20 | Con trực tiếp của GreatFurnace |
| **DayNightManager** | `DayNightManager` | `sunLight` -> `Directional Light`, `furnace` -> `GreatFurnace` | Chu kỳ 60s test, ban đêm co bán kính lò về 60% |
| **Player** | `ResourceWallet`, `FurnaceFuelInteractor` | `FurnaceFuelInteractor.furnace` -> `GreatFurnace` hoặc auto-find | Cần gắn thêm trong Inspector; bấm `E` gần Hỏa Lò để nạp củi |
| **Wood Pickup prefab/object** | `CollectibleResource` + Collider Trigger | N/A | Cần tạo vài object test quanh rìa vùng sáng |

---

## 3. DANH SÁCH FILE MÃ NGUỒN ĐÃ TẠO
1. `Assets/_Project/Features/Combat/Scripts/PlayerMovement.cs`
2. `Assets/_Project/Features/Combat/Scripts/IsometricCameraFollow.cs`
3. `Assets/_Project/Features/DayNightCycle/Scripts/GreatFurnace.cs`
4. `Assets/_Project/Features/DayNightCycle/Scripts/MistZoneDetector.cs`
5. `Assets/_Project/Features/DayNightCycle/Scripts/DayNightManager.cs`
6. `Assets/_Project/Features/Economy/Scripts/ResourceType.cs`
7. `Assets/_Project/Features/Economy/Scripts/ResourceWallet.cs`
8. `Assets/_Project/Features/Economy/Scripts/CollectibleResource.cs`
9. `Assets/_Project/Features/DayNightCycle/Scripts/FurnaceFuelInteractor.cs`

---

## 4. VẤN ĐỀ ĐÃ XỬ LÝ & LƯU Ý KỸ THUẬT (BUG LOG)
- **Crash do PowerShell:** Đã index lại thư mục an toàn, không tạo hàng loạt file khi Unity đang chạy.
- **Lỗi New Input System:** Đã chuyển `Active Input Handling` trong Project Settings sang `Both`.
- **Cảnh báo Unity 6 CS0618:** Đã chuyển toàn bộ `FindObjectsByType` sang cú pháp chuẩn `(FindObjectsInactive.Exclude)`.

---

## 5. NHIỆM VỤ TIẾP THEO (NEXT ACTIONS)
1. Gắn `ResourceWallet` và `FurnaceFuelInteractor` lên `Player`.
2. Tạo vài object củi test: Cube/Capsule nhỏ + Collider bật `Is Trigger` + `CollectibleResource(resourceType = Wood, amount = 1)`.
3. Play test loop: nhặt củi -> quay về gần Hỏa Lò -> bấm `E` để nạp -> kiểm tra `GreatFurnace.CurrentFuelSeconds` và bán kính ban đêm giữ tối thiểu 85%.
4. Sau khi loop tài nguyên ổn, triển khai **Building Grid System**: `GridManager`, preview snap-to-grid, giới hạn đặt công trình trong vùng sáng Hỏa Lò.
