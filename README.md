# Sbox Iconify

Use 200,000+ icons from any icon pack in your S&Box project.

Icons are fetched at runtime from the [Iconify API](https://iconify.design/) and cached locally.

## Usage

```html
<iconify icon="ph:house" Size=@(24) Color="white" />
<iconify icon="ph:gear" Size=@(20) Color="#c0392b" />
<iconify icon="mdi:account" Size=@(32) Color="rgb(255,255,255)" />
```

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `icon` | string | "" | Icon identifier in `prefix:name` format |
| `Color` | string | "white" | Icon color (hex, named, rgb) |
| `Size` | int | 24 | Icon size in pixels |

## Browse Icons

Find icons at [icones.js.org](https://icones.js.org/)

Popular icon packs:
- **ph** — [Phosphor Icons](https://icones.js.org/collection/ph) (1500+ icons)
- **mdi** — [Material Design Icons](https://icones.js.org/collection/mdi) (7000+ icons)
- **tabler** — [Tabler Icons](https://icones.js.org/collection/tabler) (4000+ icons)
- **lucide** — [Lucide](https://icones.js.org/collection/lucide) (1500+ icons)

## How It Works

1. First render: fetches SVG from `api.iconify.design`
2. Converts SVG to texture
3. Caches to disk (`FileSystem.Data/iconify_cache/`)
4. Subsequent loads use memory/disk cache — no network needed

## License

MIT
