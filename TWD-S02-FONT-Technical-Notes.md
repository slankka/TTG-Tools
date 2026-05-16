---

## Telltale TWD Season 02 BMFONT 参数速查表

### 1. 通用设置

| 项目 | 设置 |
|---|---|
| 字体 | 思源黑体 Heavy |
| Match char height | 不勾选 |
| Height | 100% |
| Padding | 0,0,0,0 |
| Spacing | A=1, B=1 |
| Force offsets to zero | 勾选 |

---

### 2. 字号方案速查

| 方案 | Size px | 调整后 `lineHeight` | 调整后 `base` | 目标 `xAdvance` | 目标 `max(Height)` |
|---|---:|---:|---:|---:|---:|
| 大字 | 78 | 92 | 80 | 54 | 53 |
| 小字 | 55 | 67 | 48 | 38 | 36 |

---

### 3. FNT 样例

| 方案 | 样例 |
|---|---|
| 大字 | `char id=40483 x=0 y=830 width=58 height=82 xoffset=-2 yoffset=-2 xadvance=54 page=0 chnl=15` |
| 小字 | `char id=40718 x=0 y=480 width=42 height=59 xoffset=-2 yoffset=-2 xadvance=38 page=0 chnl=15` |

---

### 4. 参数理解速查

| 参数 | 作用 | 备注 |
|---|---|---|
| Height | 主要影响字符框高度与行距 | 这里设得较高，主要是为了解决行距问题 |
| lineHeight | 控制行与行之间的垂直间距 | 需要配合实际显示效果手动调整 |
| base | 控制基线与文字实际垂直位置 | 对文字、图标对齐，以及大小字混排都很重要 |
| xAdvance | 控制字符前进宽度 | 是否接近官方字体观感的关键参数之一 |
| max(Height) | 实际显示时的最大字符高度参考 | 用来判断是否接近官方字体大小 |

---

### 5. Telltale 引擎特别注意

| 项目 | 说明 |
|---|---|
| Base 方向 | **对于 Telltale 引擎，Base 数值越大，文字在画面中的位置越高** |
| 与常见引擎区别 | 这一点和很多常见游戏引擎相反 |
| 调整经验 | 想让文字上移，就增大 `base`；想让文字下移，就减小 `base` |

---

### 6. 实战结论

| 检查项 | 目标 |
|---|---|
| 字号观感 | 尽量接近官方字体大小 |
| xAdvance | 与官方字体接近 |
| 实际显示高度 | 与官方字体接近 |
| 行距 | 不拥挤、不重叠 |
| 图标与文字对齐 | 尽量自然 |
| 大字/小字混排 | 相邻两行不重叠 |

---

### 7. 最终经验总结

- 使用 **思源黑体 Heavy**
- 保持统一基础设置：
  - 不勾选 `Match char height`
  - `Height = 100%`
  - `Padding = 0,0,0,0`
  - `Spacing = A1 B1`
  - 勾选 `Force offsets to zero`
- 再根据字号手动调整：
  - **大字：78 px → `lineHeight=92`, `base=80`**
  - **小字：55 px → `lineHeight=67`, `base=48`**
- 其中：
  - `Height` 偏高主要是为了解决行距问题
  - `Base` 主要用于文字/图标对齐，以及避免大小字相邻两行重叠
  - **在 Telltale 引擎中，`base` 越大，文字位置越高**

---
