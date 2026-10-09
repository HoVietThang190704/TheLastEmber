# TRẠNG THÁI DỰ ÁN: NGỌN LỬA TÀN (THE LAST EMBER)
Cập nhật lần cuối: 2026-10-09 | Engine: Unity 6 (6000.0.41f1) URP | Input: Both (New + Legacy)

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
- [x] Kết nối Economy MVP vào scene bằng prefab/inspector (`ResourceWallet`, `FurnaceFuelInteractor`, `CollectibleResource`) và play test thành công
- [x] Building Grid MVP scripts: grid snap, preview hợp lệ/không hợp lệ, kiểm tra vùng sáng, tiêu tài nguyên khi đặt
- [x] Kết nối Building Grid MVP vào scene bằng prefab/inspector (`GridManager`, `BuildingPlacer`, `BuildableDefinition`) và play test thành công
- [x] Combat FSM MVP scripts: trạng thái `Idle/Move/Attack/Recover`, attack input, cooldown, stamina tiêu hao/hồi phục, hit detection, dummy nhận sát thương
- [x] Play test Combat FSM MVP trong scene: `Mouse1` đánh trúng `TrainingDummy`, Console có log damage
- [x] Combat feedback MVP scripts: `TrainingDummy` nháy màu khi trúng đòn, HUD stamina hiển thị tiêu hao/hồi phục
- [ ] [ĐANG LÀM]: Play test Combat Feedback MVP trong scene
- [ ] [CHƯA LÀM]: Hệ thống Chợ Đêm & Đợt quái sương mù (Night Wave Spawner)

---

## 2. CHI TIẾT CÁC COMPONENT ĐANG HOẠT ĐỘNG TRONG SCENE
| Đối tượng (Hierarchy) | Script / Component đính kèm | Tham chiếu (References) đã nối | Ghi chú trạng thái |
| :--- | :--- | :--- | :--- |
| **Ground** | Mesh Renderer, Box/Mesh Collider | N/A | Scale: (5, 1, 5) |
| **Player** | `CharacterController`, `PlayerMovement`, `MistZoneDetector`, `CombatStamina`, `PlayerCombat` | `MistZoneDetector.furnace` -> `GreatFurnace`; `PlayerCombat.stamina` -> `CombatStamina` | Di chuyển WASD mượt mà, nhận diện ra/vào sương; `Mouse1` để đánh cận chiến |
| **Main Camera** | `Camera`, `IsometricCameraFollow` | `Target` -> `Player` | Offset: (0, 12, -8), SmoothSpeed: 5 |
| **GreatFurnace** | `GreatFurnace` | `furnaceLight` -> con `FurnaceLight` | Level 1, BaseRadius = 15m, Gizmos vàng hiển thị tốt |
| ↳ **FurnaceLight** | `Light` (Point Light) | Màu cam vàng, Intensity: 15, Range: 20 | Con trực tiếp của GreatFurnace |
| **DayNightManager** | `DayNightManager` | `sunLight` -> `Directional Light`, `furnace` -> `GreatFurnace` | Chu kỳ 60s test, ban đêm co bán kính lò về 60% |
| **Player** | `ResourceWallet`, `FurnaceFuelInteractor`, `BuildingPlacer` | `FurnaceFuelInteractor.furnace` -> `GreatFurnace`; `BuildingPlacer.gridManager` -> `BuildingSystem`; `BuildingPlacer.wallet` -> `ResourceWallet`; `selectedBuildable` -> `TorchPostDefinition` | Bấm `E` gần Hỏa Lò để nạp củi; rê chuột và click trái để đặt `TorchPost` |
| **Wood Pickup prefab/object** | `CollectibleResource` + Collider Trigger | N/A | Cần tạo vài object test quanh rìa vùng sáng |
| **BuildingSystem** | `GridManager` | `furnace` -> `GreatFurnace` | Cell Size = 2, kiểm tra vùng sáng và ô đã chiếm |
| **TorchPostDefinition** | `BuildableDefinition` asset | `prefab` -> `TorchPost` | Footprint `(1,1)`, `YOffset = 0.5`, cost `Wood = 2` |
| **TorchPost** | Prefab placeholder | Mesh Renderer, Box Collider | Đặt được bằng `BuildingPlacer` trong vùng sáng |
| **TrainingDummy** | `TrainingDummy`, Box Collider | N/A | Đặt trước Player để test `Mouse1` attack, nhận damage, nháy đỏ khi trúng đòn rồi tự hồi HP khi bị hạ |
| **CombatHUD** | `StaminaBarUI` | `stamina` -> `Player.CombatStamina` | Hiển thị thanh stamina góc trên trái bằng OnGUI |

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
10. `Assets/_Project/Features/BuildingSystem/Scripts/BuildingCost.cs`
11. `Assets/_Project/Features/BuildingSystem/Scripts/BuildableDefinition.cs`
12. `Assets/_Project/Features/BuildingSystem/Scripts/GridManager.cs`
13. `Assets/_Project/Features/BuildingSystem/Scripts/BuildingPlacer.cs`
14. `Assets/_Project/Features/Combat/Scripts/CombatState.cs`
15. `Assets/_Project/Features/Combat/Scripts/CombatStamina.cs`
16. `Assets/_Project/Features/Combat/Scripts/IDamageable.cs`
17. `Assets/_Project/Features/Combat/Scripts/PlayerCombat.cs`
18. `Assets/_Project/Features/Combat/Scripts/TrainingDummy.cs`
19. `Assets/_Project/UI/HUD/StaminaBarUI.cs`

---

## 4. VẤN ĐỀ ĐÃ XỬ LÝ & LƯU Ý KỸ THUẬT (BUG LOG)
- **Crash do PowerShell:** Đã index lại thư mục an toàn, không tạo hàng loạt file khi Unity đang chạy.
- **Lỗi New Input System:** Đã chuyển `Active Input Handling` trong Project Settings sang `Both`.
- **Cảnh báo Unity 6 CS0618:** Đã chuyển toàn bộ `FindObjectsByType` sang cú pháp chuẩn `(FindObjectsInactive.Exclude)`.
- **Git/GitHub:** Đã thêm `.gitignore` chuẩn Unity, tạo commit đầu tiên và push nhánh `main` lên `https://github.com/HoVietThang190704/TheLastEmber.git`.
- **Building Grid MVP:** Đã tạo `TorchPost.prefab`, `TorchPostDefinition.asset`, nối `GridManager`/`BuildingPlacer` vào scene; play test pass preview xanh/đỏ, click đặt công trình, trừ `Wood`, giới hạn trong vùng sáng.
- **Combat FSM MVP:** Đã thêm `CombatStamina`, `PlayerCombat`, `IDamageable`, `TrainingDummy`; nối `CombatStamina`/`PlayerCombat` vào Player, thêm `TrainingDummy` trong scene; `dotnet build Assembly-CSharp.csproj` pass 0 errors.
- **Combat Play Test:** Player dùng `Mouse1` đánh trúng `TrainingDummy`; Console xác nhận dummy nhận damage.
- **Combat Feedback MVP:** Đã thêm nháy màu đỏ cho `TrainingDummy` khi nhận damage và `CombatHUD` hiển thị stamina.

---

## 5. NHIỆM VỤ TIẾP THEO (NEXT ACTIONS)
1. Play test Combat Feedback MVP: bấm Play, dùng `Mouse1` đánh `TrainingDummy`, kiểm tra dummy nháy đỏ và thanh stamina góc trên trái tụt/hồi.
2. Nếu pass: commit mốc combat feedback.
3. Sau đó chuyển sang Night Wave Spawner MVP: spawn enemy sương mù theo phase `Night`, tiến về Hỏa Lò/Player.
