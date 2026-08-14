# Elysium Mall — Unity/C# Portfolio Prototype

This prototype has three connected portfolio demonstrations:

1. **Speed Circuit** — a playable obstacle-course attraction with movement, dash, respawn, patrolling hazards, wandering hazards, projectiles, spike traps, and a goal gate.
2. **Shopping Area** — a separate mall zone with interactive product kiosks populated from JSON catalog data fetched through a local HTTP endpoint.
3. **AI Concierge** — profile-driven recommendations fetched through an AI-service-shaped JSON endpoint, with an offline fallback that keeps the client usable.

The visual direction is intentionally lightweight: primitive 3D architecture, flat 1990s-corporate colors, orthographic-style framing, and generated pixel sprites presented as camera-facing billboards. The generated sprites are stand-ins; replace them with properly licensed or original art before public distribution.

## Run it

Open this folder in Unity **2022.3.22f1** and open `Assets/Scenes/Main.unity`.

For the HTTP JSON path, start the local catalog service from this project directory:

```bash
python3 Tools/catalog_server.py
```

The Unity client requests `http://127.0.0.1:8766/catalog.json` and `recommendations.json`. If the service is unavailable, it uses the same JSON from `Assets/Resources/catalog.json` as a deterministic fallback.

Pass a different port as the first argument if `8766` is already occupied, then update `MallApp.catalogEndpoint` in the Inspector.

## Controls

- `WASD` or arrow keys: move
- Left stick or D-pad: move when a PS3/PS2 USB adapter is exposed as a joystick
- `Space`: dash
- Controller button 0: dash
- `E`: inspect a nearby shopping kiosk
- Controller button 1: inspect a nearby shopping kiosk
- `Escape`: close the product card
- `EXPLORE`, `STYLE`, and `TECH`: change the recommendation profile
- `F11`: fullscreen in a standalone build; maximize the Game view in the Unity Editor for testing
- Use the UI buttons to switch between the two areas

## Evidence map

| Job skill | Demonstrated by |
| --- | --- |
| Unity/C# | Runtime scene construction, components, player controller, hazards, UI |
| REST/JSON | `CatalogService`, local HTTP catalog endpoint, fallback handling |
| AI-service integration | Profile-driven recommendation endpoint, rationale/model metadata, offline fallback |
| Real-time environment | Traversable mall and course, portals, collision, interaction |
| UI | Status bar, navigation buttons, product detail card |
| Sprite/3D workflow | Generated pixel sprites, billboards, primitive 3D environment |
| Maintainability | Small focused scripts and reusable runtime factories |
| Optimization path | Low-poly geometry, shared materials, generated sprite cache, documented baseline target |
| WebGL path | No native plugins or file-system dependency in the runtime path |

## Cookbook provenance

The obstacle-course structure is an original implementation inspired by the planning topics in Casey Hardman, *Game Programming with Unity and C#*, Chapter 13, “Obstacle Course Design and Outline” (2020), DOI: https://doi.org/10.1007/978-1-4842-5656-5_13. The implementation here is not copied chapter code.

## Portfolio next steps

1. Capture Unity Profiler baseline and optimized runs with the same course traversal.
2. Add an automated test assembly for catalog parsing, movement state, respawn, and product selection.
3. Add a WebGL build and short demo video.
4. Replace the generated fan-style placeholders with original or licensed sprites for a public-facing release.

## License

Released under the MIT License. See [LICENSE](LICENSE).
