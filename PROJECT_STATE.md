# TRẠNG THÁI DỰ ÁN: NGỌN LỬA TÀN (THE LAST EMBER)
Cập nhật lần cuối: 2026-10-10 | Engine: Unity 6 (6000.0.41f1) URP | Input: Both (New + Legacy)

---

## 1. TIẾN ĐỘ TỔNG THỂ
- [x] Khởi tạo cấu trúc thư mục chuẩn Feature-driven (`Assets/_Project/...`)
- [x] Dựng Graybox cơ bản (`Ground`, `Player` Capsule, `Main Camera`)
- [x] Thay `Ground` plane bằng model `dat.glb` đã convert mesh-data và tự đổ màu đất/cỏ/đá qua `GroundModelVisual.cs`
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
- [x] Play test Combat Feedback MVP trong scene: dummy nháy đỏ, stamina tụt/hồi, Console có log damage
- [x] Commit mốc Combat Feedback MVP
- [x] Night Wave Spawner MVP scripts: spawn quái sương mù vào ban đêm, quái đuổi Player, nhận damage qua `IDamageable`
- [x] Play test Night Wave Spawner MVP trong scene: ban đêm spawn quái, quái đuổi Player, nhận damage, nháy màu và bị destroy khi hết HP
- [x] Commit mốc Night Wave Spawner MVP
- [x] Player Health + enemy contact damage MVP scripts: Player có máu, HUD máu, quái gây sát thương theo cooldown khi chạm Player
- [x] Play test Player Health + enemy contact damage MVP trong scene: quái gây damage theo cooldown, HUD máu giảm, HP về 0 thì reset full cho MVP test
- [x] Commit mốc Player Health + enemy contact damage MVP
- [x] Player hurt feedback + death/fail state MVP scripts: Player nháy màu khi bị đánh, knockback nhẹ, tạm khóa di chuyển/đánh khi HP về 0 rồi respawn
- [x] Play test Player hurt feedback + death/fail state MVP trong scene: Player nháy đỏ, knockback nhẹ, down state khóa input, respawn sau 1.5s
- [x] Commit mốc Player hurt feedback MVP
- [x] Night Wave Reward/Drop MVP scripts: `MistEnemy` rơi `SilverEmber` khi chết, `NightWaveSpawner` truyền `ResourceWallet` vào enemy khi spawn, drop amount chỉnh được qua Inspector
- [x] Play test Night Wave Reward/Drop MVP trong scene: đánh chết quái → Console log vàng drop + log xanh tài nguyên tăng
- [x] Commit mốc Night Wave Reward/Drop MVP
- [x] HUD SilverEmber MVP script: hiển thị số dư Silver Ember dưới thanh Health, cập nhật real-time qua event `ResourceChanged` (`SilverEmberUI.cs`)
- [x] Play test HUD SilverEmber trong scene: gắn `SilverEmberUI` vào `CombatHUD`, xác nhận badge hiển thị đúng số sau khi quái chết
- [x] Commit mốc HUD SilverEmber MVP
- [x] Furnace Upgrade MVP scripts: `GreatFurnace` có `Upgrade()`, `GetUpgradeCost()`, event `LevelChanged`, max level 5; `FurnaceUpgradeInteractor` nhấn `U` gần lò tiêu `SilverEmber`; `FurnaceLevelUI` hiển thị level trên HUD
- [x] Play test Furnace Upgrade MVP: gắn `FurnaceUpgradeInteractor` vào Player, `FurnaceLevelUI` vào `CombatHUD`, đứng gần lò nhấn `U` khi có ≥ 5 SilverEmber → Level 2, bán kính tăng
- [x] Commit mốc Furnace Upgrade MVP
- [x] Enemy Wave Scaling MVP scripts: `DayNightManager` đếm `NightCount`; `NightWaveSpawner` tính scaling theo đêm (max quái, interval, stat multiplier); `MistEnemy.Initialize()` nhận `statMultiplier` scale HP/damage/speed
- [x] Kenney asset selection/import pass: chọn lọc import FBX hợp game từ `kenney_survival-kit.zip` và `kenney_nature-kit.zip` vào `Assets/ThirdParty/Kenney/...`; thêm Editor bootstrapper sinh prefab gameplay từ model Kenney.
- [ ] Kenney prefab generation/import validation trong Unity Editor: mở Unity để import FBX đầy đủ và để `KenneyAssetBootstrapper` tự tạo/refresh `WoodPickup`, `StonePickup`, `TorchPost`, buildable definitions và environment prop prefabs.
- [x] Terrain/HUD visual polish pass: dịu palette mặt đất, thêm decor cây/đá/log tự sinh quanh vùng chơi qua `GroundModelVisual`, làm HUD OnGUI bớt gắt bằng shadow/viền mờ/highlight.
- [x] Resource HUD MVP: HUD hiển thị Wood/Stone/SilverEmber trực tiếp từ `ResourceWallet`, cập nhật real-time khi nhặt/tiêu tài nguyên.
- [x] Play test Resource HUD MVP: nhặt tài nguyên trong Play Mode, HUD cập nhật số Wood/Stone/SilverEmber đúng.
- [x] Interaction Prompt MVP scripts/scene hookup: thêm prompt HUD chỉ hiện khi đứng gần Hỏa Lò, báo `E` nạp củi và `U` nâng cấp theo trạng thái tài nguyên.
- [x] Play test Interaction Prompt MVP: đứng gần Hỏa Lò, prompt hiển thị đúng trạng thái nạp củi/nâng cấp.
- [x] Build Hotkey MVP scripts/scene hookup: phím `1/2/3` chọn Torch Post/Campfire/Fortified Fence, HUD hiển thị lựa chọn và cost.
- [x] Play test Build Hotkey MVP: hotkey `1/2/3`, preview, cost HUD và trừ tài nguyên khi đặt đều pass.
- [ ] Play test Enemy Wave Scaling: chạy qua 2-3 đêm, xác nhận Console log đêm thứ N với quái căng hơn
- [ ] Commit mốc Enemy Wave Scaling MVP

---

## 2. CHI TIẾT CÁC COMPONENT ĐANG HOẠT ĐỘNG TRONG SCENE
| Đối tượng (Hierarchy) | Script / Component đính kèm | Tham chiếu (References) đã nối | Ghi chú trạng thái |
| :--- | :--- | :--- | :--- |
| **Ground** | `GroundModelVisual`, Mesh Renderer, Mesh Collider | `resourcePath` -> `Resources/Ground/dat_colored_mesh` | Dùng source `Assets/_Project/Resources/Ground/dat.glb`; script dựng mesh runtime, gán 5 material đất/cỏ/đá, Scale: (25, 0.18, 25) |
| **Player** | `CharacterController`, `PlayerMovement`, `MistZoneDetector`, `CombatStamina`, `PlayerCombat`, `PlayerHealth`, `PlayerHurtFeedback` | `MistZoneDetector.furnace` -> `GreatFurnace`; `PlayerCombat.stamina` -> `CombatStamina`; `PlayerMovement.health`/`PlayerCombat.health` -> `PlayerHealth`; `PlayerHurtFeedback.health` -> `PlayerHealth` | Di chuyển WASD mượt mà, nhận diện ra/vào sương; `Mouse1` để đánh cận chiến; nhận damage từ quái; nháy màu/knockback khi bị đánh |
| **Main Camera** | `Camera`, `IsometricCameraFollow` | `Target` -> `Player` | Offset: (0, 12, -8), SmoothSpeed: 5 |
| **GreatFurnace** | `GreatFurnace` | `furnaceLight` -> con `FurnaceLight` | Level 1, BaseRadius = 15m, Gizmos vàng hiển thị tốt |
| ↳ **FurnaceLight** | `Light` (Point Light) | Màu cam vàng, Intensity: 15, Range: 20 | Con trực tiếp của GreatFurnace |
| **DayNightManager** | `DayNightManager` | `sunLight` -> `Directional Light`, `furnace` -> `GreatFurnace` | Chu kỳ 60s test, ban đêm co bán kính lò về 60% |
| **Player** | `ResourceWallet`, `FurnaceFuelInteractor`, `BuildingPlacer` | `FurnaceFuelInteractor.furnace` -> `GreatFurnace`; `BuildingPlacer.gridManager` -> `BuildingSystem`; `BuildingPlacer.wallet` -> `ResourceWallet`; hotkeys `1/2/3` -> TorchPost/Campfire/FortifiedFence | Bấm `E` gần Hỏa Lò để nạp củi; phím `1/2/3` chọn công trình, rê chuột và click trái để đặt |
| **Wood Pickup prefab/object** | `CollectibleResource` + Collider Trigger | N/A | Cần tạo vài object test quanh rìa vùng sáng |
| **BuildingSystem** | `GridManager` | `furnace` -> `GreatFurnace` | Cell Size = 2, kiểm tra vùng sáng và ô đã chiếm |
| **TorchPostDefinition** | `BuildableDefinition` asset | `prefab` -> `TorchPost` | Footprint `(1,1)`, `YOffset = 0.5`, cost `Wood = 2` |
| **TorchPost** | Prefab placeholder | Mesh Renderer, Box Collider | Đặt được bằng `BuildingPlacer` trong vùng sáng |
| **TrainingDummy** | `TrainingDummy`, Box Collider | N/A | Đặt trước Player để test `Mouse1` attack, nhận damage, nháy đỏ khi trúng đòn rồi tự hồi HP khi bị hạ |
| **CombatHUD** | `StaminaBarUI`, `HealthBarUI`, `SilverEmberUI`, `FurnaceLevelUI`, `FurnaceInteractionPromptUI`, `BuildSelectionUI` | `stamina` -> `Player.CombatStamina`; `health` -> `Player.PlayerHealth`; resource/furnace/interactor/build placer auto-find nếu chưa nối | Hiển thị health, stamina, Wood/Stone/SilverEmber, Furnace level, prompt Hỏa Lò và panel chọn xây dựng bằng OnGUI |
| **NightWaveSystem** | `NightWaveSpawner` | `dayNightManager` -> `DayNightManager`; `furnace` -> `GreatFurnace`; `target` -> `Player` | Ban đêm spawn `MistEnemy` placeholder quanh vùng sáng; hết đêm despawn |

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
20. `Assets/_Project/Features/NightMarket/Scripts/MistEnemy.cs`
21. `Assets/_Project/Features/NightMarket/Scripts/NightWaveSpawner.cs`
22. `Assets/_Project/Features/Combat/Scripts/PlayerHealth.cs`
23. `Assets/_Project/UI/HUD/HealthBarUI.cs`
24. `Assets/_Project/Features/Combat/Scripts/PlayerHurtFeedback.cs`
  *(đã chỉnh sửa)* `Assets/_Project/Features/NightMarket/Scripts/MistEnemy.cs` — thêm drop SilverEmber khi chết
  *(đã chỉnh sửa)* `Assets/_Project/Features/NightMarket/Scripts/NightWaveSpawner.cs` — thêm ResourceWallet reference, truyền vào Initialize()
25. `Assets/_Project/UI/HUD/SilverEmberUI.cs`
  *(đã chỉnh sửa)* `Assets/_Project/Features/DayNightCycle/Scripts/GreatFurnace.cs` — thêm `maxLevel`, `upgradeCosts[]`, `Upgrade()`, `GetUpgradeCost()`, event `LevelChanged`
26. `Assets/_Project/Features/DayNightCycle/Scripts/FurnaceUpgradeInteractor.cs`
27. `Assets/_Project/UI/HUD/FurnaceLevelUI.cs`
  *(đã chỉnh sửa)* `Assets/_Project/Features/DayNightCycle/Scripts/DayNightManager.cs` — thêm `NightCount`, log đêm mới
  *(đã chỉnh sửa)* `Assets/_Project/Features/NightMarket/Scripts/MistEnemy.cs` — thêm `statMultiplier` vào `Initialize()`
  *(đã chỉnh sửa)* `Assets/_Project/Features/NightMarket/Scripts/NightWaveSpawner.cs` — thêm Wave Scaling config, `ApplyWaveScaling()`
28. `Assets/_Project/Features/Environment/Scripts/GroundModelVisual.cs`
  *(đã thêm)* `Assets/_Project/Resources/Ground/dat.glb` — source model mặt đất
  *(đã thêm)* `Assets/_Project/Resources/Ground/dat_colored_mesh.bytes` — mesh-data convert từ `dat.glb`, chia submesh để đổ màu đất/cỏ/đá
29. `Assets/_Project/Editor/KenneyAssetBootstrapper.cs`
  *(đã thêm)* `Assets/ThirdParty/Kenney/SurvivalKit/...` — FBX chọn lọc: resource wood/stone, campfire, fence, chest, barrel, tools, workbench
  *(đã thêm)* `Assets/ThirdParty/Kenney/NatureKit/...` — FBX chọn lọc: cây, log, stump, rock/stone, mushroom, bush, grass, stone path
  *(đã chỉnh sửa)* `Assets/_Project/Features/Environment/Scripts/GroundModelVisual.cs` — palette terrain mới + decor cây/đá/log procedural
  *(đã chỉnh sửa)* `Assets/_Project/UI/HUD/StaminaBarUI.cs`, `HealthBarUI.cs`, `SilverEmberUI.cs`, `FurnaceLevelUI.cs` — HUD shadow/viền/fill polish
  *(đã chỉnh sửa)* `Assets/Scenes/SampleScene.unity` — bật terrain decorations và cập nhật màu HUD serialize
30. `Assets/_Project/UI/HUD/FurnaceInteractionPromptUI.cs`
31. `Assets/_Project/UI/HUD/BuildSelectionUI.cs`

---

## 4. VẤN ĐỀ ĐÃ XỬ LÝ & LƯU Ý KỸ THUẬT (BUG LOG)
- **Crash do PowerShell:** Đã index lại thư mục an toàn, không tạo hàng loạt file khi Unity đang chạy.
- **Lỗi New Input System:** Đã chuyển `Active Input Handling` trong Project Settings sang `Both`.
- **Cảnh báo Unity 6 CS0618:** Đã chuyển toàn bộ `FindObjectsByType` sang cú pháp chuẩn `(FindObjectsInactive.Exclude)`.
- **Git/GitHub:** Đã thêm `.gitignore` chuẩn Unity, tạo commit đầu tiên và push nhánh `main` lên `https://github.com/HoVietThang190704/TheLastEmber.git`.
- **Building Grid MVP:** Đã tạo `TorchPost.prefab`, `TorchPostDefinition.asset`, nối `GridManager`/`BuildingPlacer` vào scene; play test pass preview xanh/đỏ, click đặt công trình, trừ `Wood`, giới hạn trong vùng sáng.
- **Combat FSM MVP:** Đã thêm `CombatStamina`, `PlayerCombat`, `IDamageable`, `TrainingDummy`; nối `CombatStamina`/`PlayerCombat` vào Player, thêm `TrainingDummy` trong scene; `dotnet build Assembly-CSharp.csproj` pass 0 errors.
- **Combat Play Test:** Player dùng `Mouse1` đánh trúng `TrainingDummy`; Console xác nhận dummy nhận damage.
- **Combat Feedback MVP:** Đã thêm nháy màu đỏ cho `TrainingDummy` khi nhận damage và `CombatHUD` hiển thị stamina; play test pass.
- **Night Wave Spawner MVP:** Đã thêm `MistEnemy` và `NightWaveSpawner`; nối `NightWaveSystem` vào scene để spawn quái khi phase là `Night`; play test pass: wave started/spawned, `MistEnemy` đuổi Player, nhận damage qua `Mouse1`, nháy màu và bị destroy khi hết HP.
- **Player Health MVP:** Đã thêm `PlayerHealth`, `HealthBarUI`; cập nhật `MistEnemy` để gây contact damage qua `IDamageable` theo cooldown, không trừ máu mỗi frame; play test pass: HUD máu giảm khi bị chạm, HP về 0 reset full cho MVP test.
- **Player Hurt Feedback MVP:** Đã thêm `PlayerHurtFeedback`; `PlayerHealth` có trạng thái `IsDead`, delay respawn 1.5s, event damage/death/respawn; `PlayerMovement` và `PlayerCombat` tạm khóa input khi Player chết; play test pass: Player nháy đỏ, knockback nhẹ, down state khóa input, respawn đúng nhịp.
- **Night Wave Reward/Drop MVP:** Commit `4c10812f`— quái rơi SilverEmber khi chết, play test pass.
- **HUD SilverEmber MVP:** Commit `5b74df05` — badge gold hiển thị số dư, cập nhật real-time, play test pass.
- **Furnace Upgrade MVP:** Commit `08b25722` — nhấn `U` tiêu SilverEmber nâng lên Level 2-5, bán kính tăng, HUD cập nhật, play test pass.
- **Ground model/color pass:** Đã đưa `dat.glb` vào `Resources/Ground`, convert sang mesh-data runtime, thêm `GroundModelVisual` tự tạo material URP đất/cỏ/sỏi/đá và gắn vào `Ground` trong `SampleScene`.
- **Kenney asset application pass:** Đã import chọn lọc asset hợp gameplay từ `SurvivalKit` và `NatureKit`; thêm `KenneyAssetBootstrapper` chạy sau import hoặc qua menu `The Last Ember/Kenney/Rebuild Applied Asset Prefabs` để tạo prefab `WoodPickup`, `StonePickup`, `TorchPost`, `Campfire`, `FortifiedFence`, `Workbench`, decor môi trường.
- **Unity batchmode limitation:** Thử chạy Unity batchmode để generate prefab ngay nhưng bị kẹt Unity Licensing Client/headless package; đã dừng tiến trình batchmode. `dotnet build Assembly-CSharp-Editor.csproj` pass 0 errors, prefab generation sẽ chạy khi mở Unity Editor có license bình thường.
- **Terrain/HUD polish:** `GroundModelVisual` đổi bảng màu đất/cỏ/đá sang tông dịu hơn và tự sinh cây/đá/log ở vành ngoài vùng Hỏa Lò; HUD OnGUI có shadow nhẹ, viền màu đồng mờ, fill highlight. `dotnet build Assembly-CSharp.csproj` pass 0 warnings/0 errors.
- **Fix terrain decor bị bẹt/xa:** Decor procedural ban đầu parent trực tiếp vào `Ground` nên bị nhân scale `(25, 0.18, 25)`, làm cây/đá/log bị ép dẹt và văng xa. Đã sửa `GroundModelVisual` để root decor dùng inverse local scale của `Ground`; `dotnet build Assembly-CSharp.csproj` pass 0 errors.
- **Procedural log pickup:** Các khúc gỗ procedural trong `GroundModelVisual` giờ có trigger collider + `CollectibleResource(Wood)`, nhặt được `woodPerLog` Wood rồi biến mất; cây đứng/đá vẫn là decor. `CollectibleResource` có thêm `Configure()` để runtime setup resource.
- **Wood pickup visual:** Các `WoodPickup_01` cũ trong scene vốn đã là `CollectibleResource(Wood)` nhưng còn dùng cube placeholder. Đã cho `CollectibleResource` tự tạo visual tài nguyên trong Editor/Play Mode: Wood thành cụm khúc gỗ, Stone thành đá, SilverEmber thành lõi phát sáng; prefab Kenney có model sẵn thì giữ nguyên model, log procedural không bị nhân đôi visual. `dotnet build Assembly-CSharp.csproj` và `Assembly-CSharp-Editor.csproj` pass 0 errors.
- **Resource HUD MVP:** `SilverEmberUI` hiện đã vẽ panel tài nguyên đầy đủ `Wood / Stone / Ember` trên `CombatHUD`, đọc từ `ResourceWallet` và cập nhật qua event `ResourceChanged`. `SampleScene` đã tăng badge width lên 316px để hiển thị đủ 3 loại; `dotnet build Assembly-CSharp.csproj` pass 0 errors.
- **Resource HUD play test:** Người dùng xác nhận Resource HUD đã pass trong Play Mode.
- **Interaction Prompt MVP:** Thêm `FurnaceInteractionPromptUI` vào `CombatHUD`. Prompt chỉ hiện khi Player đứng trong tầm tương tác Hỏa Lò, dùng trạng thái từ `FurnaceFuelInteractor`/`FurnaceUpgradeInteractor` để hiển thị nạp củi, thiếu Wood, nhiên liệu đầy, nâng cấp, thiếu Ember hoặc max level. `FurnaceLevelUI` giờ chỉ hiển thị level để tránh trùng prompt. `dotnet build Assembly-CSharp.csproj` pass 0 errors.
- **Interaction Prompt play test:** Người dùng xác nhận prompt tương tác Hỏa Lò đã pass trong Play Mode.
- **Build Hotkey MVP:** `BuildingPlacer` có hotkey slots và public state cho HUD. `SampleScene` gắn `1` -> Torch Post, `2` -> Campfire, `3` -> Fortified Fence; mặc định không chọn công trình để người chơi chủ động chọn. Thêm `BuildSelectionUI` vào `CombatHUD` để hiển thị công trình đang chọn và cost. `dotnet build Assembly-CSharp.csproj` pass 0 errors.
- **Build Hotkey play test:** Người dùng xác nhận chọn công trình bằng hotkey, preview/cost HUD và đặt công trình đã pass trong Play Mode.

---

## 5. NHIỆM VỤ TIẾP THEO (NEXT ACTIONS)
1. Commit mốc **Resource HUD + Interaction Prompt + Build Hotkey**.
2. Nếu cây quá dày/thưa, chỉnh ngay trên `GroundModelVisual`: `treeCount`, `rockCount`, `logCount`, `innerClearRadius`, `outerRadius`.
3. **Play test Enemy Wave Scaling**: chạy qua 2-3 đêm trong Unity, xác nhận Console log `Đêm thứ N bắt đầu. Quái: X, Interval: Ys, Stat x1.XX`.
