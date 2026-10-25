from PIL import Image, ImageDraw, ImageFont
import numpy as np
import os

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

# --- 512x512 logo ---
logo = make_icon(512, "PL+")
logo.save(os.path.join(BASE, "Heads", "Logo-CE.png"))

# --- multi-size ICO ---
base = make_icon(256, "PL+")
base.save(os.path.join(BASE, "icon.ico"), format="ICO",
          sizes=[(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)])
print("icon.ico + Logo-CE.png regenerated")
