# Mèo Thủ Thành

Game **tower defense 2D** làm bằng Unity: bạn chỉ huy một thành mèo, đặt các chiến binh mèo dọc con đường để chặn những đợt chuột kéo vào lâu đài.

- **Engine:** Unity 6000.3.28f1 (Unity 6.3 LTS)
- **Thể loại:** Tower Defense 2D, 1 màn chơi, 20 đợt
- **Đồ hoạ / âm thanh:** 100% sinh bằng code lúc chạy (không dùng asset ngoài)
- **Bản chạy sẵn:** `Builds/Windows/MeoThuThanh.exe`

---

## 1. Chạy game ngay

Mở thư mục `Builds/Windows/` và chạy **`MeoThuThanh.exe`**.

Không cần cài Unity. Khuyến nghị cửa sổ 1600×900 hoặc lớn hơn.

## 2. Cách chơi

Bạn có **12 mạng**. Chuột đi từ hang (góc trái trên) tới lâu đài (bên phải).
Mỗi con chuột chạm thành sẽ lấy đi 1 mạng (chuột đầu đàn lấy 3 mạng).
Sống qua **20 đợt** là thắng.

1. Bấm thẻ mèo ở thanh dưới (hoặc phím **1–4**).
2. Di chuột lên bãi cỏ — vòng xanh là tầm bắn, ô xanh là chỗ đặt được.
3. Bấm chuột trái để đặt mèo (tốn cá).
4. Bấm vào mèo đã đặt để xem thông tin, **nâng cấp** hoặc **bán** (hoàn 60%).
5. Bấm **GỌI ĐỢT** (hoặc **Space**) để gọi đợt sớm — gọi càng sớm càng được thưởng nhiều cá.
6. Hạ gục chuột và qua đợt để nhận thêm cá.

### Điều khiển

| Phím / chuột | Tác dụng |
|---|---|
| Chuột trái | Đặt mèo / chọn mèo / bấm nút |
| Chuột phải, `Esc` | Huỷ đặt, bỏ chọn |
| `1` `2` `3` `4` | Chọn nhanh 4 loại mèo |
| `Space` | Gọi đợt tiếp theo |
| `P` | Tạm dừng / tiếp tục |
| `M` | Bật / tắt âm thanh |
| `R` | Chơi lại (khi đã thắng hoặc thua) |
| Nút `x1/x2` | Tăng tốc trận đấu |

### 4 chiến binh mèo

| Mèo | Giá | Sát thương | Tầm | Tốc độ bắn | Đặc điểm |
|---|---:|---:|---:|---|---|
| **Mèo Mướp** | 60 | 9 → 24 | 2.7 → 3.2 | 0.85s → 0.65s | Bắn len liên tục, rẻ, ổn định |
| **Mèo Xiêm** | 90 | 6 → 15 | 2.9 → 3.3 | 1.0s → 0.8s | Đạn băng làm chậm 35–55% |
| **Mèo Ba Tư** | 120 | 18 → 42 | 3.0 → 3.4 | 1.5s → 1.2s | Bong bóng nổ sát thương diện rộng |
| **Mèo Anh** | 150 | 55 → 120 | 6.5 → 7.6 | 2.2s → 1.8s | Bắn tỉa tầm rất xa, sát thương lớn |

Mỗi mèo nâng được **3 cấp**; nâng cấp tốn cá nhưng tăng mạnh sát thương/tầm bắn.

### Kẻ địch

| Chuột | Máu | Tốc độ | Thưởng | Ghi chú |
|---|---:|---:|---:|---|
| Chuột Nhắt | 32 | 1.7 | 6 | Xuất hiện từ đầu |
| Chuột Lướt | 24 | 3.3 | 7 | Rất nhanh, máu thấp |
| Chuột Cống | 120 | 1.25 | 13 | Trâu bò, kháng làm chậm 30% |
| Chuột Đầu Đàn | 900 | 0.95 | 90 | Trùm, lấy 3 mạng |

Máu kẻ địch tăng dần **+14% mỗi đợt**. Trùm xuất hiện ở **đợt 10, 15, 19 và 20**.

### Mẹo

- Đặt **Mèo Xiêm** ở khúc cua để làm chậm cả đoàn, rồi thêm **Mèo Ba Tư** nổ diện rộng.
- **Mèo Anh** rất đáng tiền ở giữa bản đồ vì tầm bắn phủ nhiều đoạn đường.
- Giữ 1–2 chỗ trống gần cuối đường để đặt thêm khi chuột đã lọt qua.
- Đừng gọi đợt quá sớm nếu chưa kịp nâng cấp — nhưng gọi sớm cho rất nhiều cá.

---

## 3. Mở project trong Unity

1. Mở **Unity Hub** → **Add** → **Add project from disk**.
2. Chọn thư mục `MeoThuThanh` (thư mục chứa `Assets`, `Packages`, `ProjectSettings`).
3. Mở bằng **Unity 6000.3.28f1**.
4. Mở scene `Assets/Scenes/Main.unity` và bấm **Play**.

> Nếu Unity hỏi nâng cấp project, chọn đúng phiên bản 6000.3.28f1 để tránh lỗi định dạng.

## 4. Build lại bản Windows

Trong Unity: **File → Build Profiles → Windows** → **Build**.
Hoặc chạy lệnh (đóng Unity trước):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe" `
  -batchmode -nographics -quit `
  -projectPath "C:\Users\admin\OpenCode-lam-viec\MeoThuThanh" `
  -executeMethod ProjectSetup.SetupAndBuild `
  -logFile build.log
```

Kết quả nằm ở `Builds/Windows/MeoThuThanh.exe`.

## 5. Cấu trúc project

```
MeoThuThanh/
├─ Assets/
│  ├─ Scenes/Main.unity          # Scene duy nhất, chỉ chứa Bootstrap
│  ├─ Scripts/
│  │  ├─ Bootstrap.cs            # Tạo camera, hệ thống, thế giới khi chạy
│  │  ├─ Core/                   # Map, đợt, chỉ số, quản lý game, camera
│  │  ├─ Gameplay/               # Tháp, chuột, đạn, hiệu ứng, đặt tháp
│  │  ├─ UI/                     # HUD, shop, bảng nâng cấp, kết quả
│  │  ├─ Art/                    # Sinh sprite bằng code (mèo, chuột, map, icon)
│  │  ├─ Audio/                  # Nhạc nền + hiệu ứng procedural
│  │  └─ DevCapture.cs           # Chụp ảnh/demo qua command line (chỉ để test)
│  └─ Editor/ProjectSetup.cs     # Tạo scene + build Windows bằng lệnh
├─ Packages/manifest.json
├─ ProjectSettings/
├─ Builds/Windows/               # Bản chạy .exe + thư mục Data
├─ README.md
└─ HUONG-DAN-NOP-BAI.md
```

Toàn bộ hình ảnh (mèo, chuột, lâu đài, cỏ, UI) được vẽ bằng `Texture2D`/`Sprite` trong thư mục `Art/`.
Âm thanh được tổng hợp bằng code trong `Audio/AudioKit.cs`.

## 6. Ghi chú kỹ thuật

- Scene chỉ có một GameObject `Bootstrap`; mọi thứ khác sinh lúc runtime.
- Dùng **UGUI legacy Text** + font hệ thống (Segoe UI/Arial) nên hiển thị đầy đủ tiếng Việt.
- Input Handler: **Old (Input Manager)**.
- Đường đi là polyline trên lưới 16×9; không thể đặt mèo lên đường.
- `DevCapture` chỉ hoạt động khi truyền tham số dòng lệnh, người chơi bình thường không bị ảnh hưởng:
  `-demo`, `-demolevel 0|1|2`, `-shotdir <thư mục>`, `-shots "3;6;9"`, `-demoend win|lose`, `-demoselect col,row`.

---

Chúc bạn thủ thành vui vẻ! 🐱🏰
