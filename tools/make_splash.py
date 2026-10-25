from PIL import Image, ImageDraw, ImageFont
import numpy as np
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
BASE = r"E:\DeepSeekHarnessworkspace\work\PCL-CE\Plain Craft Launcher 2\Images"

FONT_CANDIDATES = [
    r"C:\Windows\Fonts\arialbd.ttf",
    r"C:\Windows\Fonts\seguisb.ttf",
    r"C:\Windows\Fonts\segoeuib.ttf",
]


def load_font(px):
    for f in FONT_CANDIDATES:
        try:
            return ImageFont.truetype(f, px)
        except Exception:
            continue
    return ImageFont.load_default()


def make_icon(size, text):
    c1 = np.array([47, 128, 255], dtype=np.float64)
    c2 = np.array([139, 92, 246], dtype=np.float64)
    x = np.linspace(0.0, 1.0, size)
    y = np.linspace(0.0, 1.0, size)
    X, Y = np.meshgrid(x, y)
    t = np.clip(X * 0.45 + Y * 0.55, 0, 1)[..., None]
    arr = (c1[None, None, :] * (1 - t) + c2[None, None, :] * t).astype(np.uint8)
    img = Image.fromarray(arr, "RGB").convert("RGBA")

    mask = Image.new("L", (size, size), 0)
    ImageDraw.Draw(mask).rounded_rectangle(
        [0, 0, size - 1, size - 1], radius=max(2, int(size * 0.22)), fill=255
    )
    img.putalpha(mask)

    d = ImageDraw.Draw(img)
    fs = max(7, int(size * (0.46 if len(text) <= 3 else 0.38)))
    font = load_font(fs)
    bbox = d.textbbox((0, 0), text, font=font)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    d.text(((size - tw) / 2 - bbox[0], (size - th) / 2 - bbox[1]), text,
           font=font, fill=(255, 255, 255, 255))
    return img


# 启动画面：200x200（WPF SplashScreen 按 96 DPI 显示，200 DIP 在高分屏上也够明显）
splash = make_icon(200, "PL+")
splash.save(os.path.join(BASE, "Heads", "Splash.png"))

# 程序图标：多尺寸，从大到小写入，确保首帧为 256
base = make_icon(256, "PL+")
sizes = [(256, 256), (128, 128), (64, 64), (48, 48), (32, 32), (24, 24), (16, 16)]
base.save(os.path.join(BASE, "icon.ico"), format="ICO", sizes=sizes)
print("Splash.png(200) + icon.ico(大尺寸优先) 已生成")
