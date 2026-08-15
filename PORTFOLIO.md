# Mall Demo portfolio evidence

This project is a reusable Unity vertical slice for portfolio demonstrations, interviews, and future interactive projects. It is intentionally small enough to explain in an interview and structured so each additional demonstration can become a production-style feature.

## Current demonstrations

| Unity developer requirement | Evidence in this project | Status |
| --- | --- | --- |
| Unity/C# scenes, components, interaction | Runtime-built mall, playable course, CharacterController, kiosks, portals, hazards | Implemented; standalone build and windowed capture verified |
| REST APIs and JSON | `CatalogService`, `/catalog.json`, error fallback, CORS headers | Implemented and smoke-tested |
| AI service integration | `/recommendations.json`, profile buttons, rationale/model metadata, offline adapter | Implemented as deterministic service seam |
| UI | Status bar, navigation, catalog status, product card, AI Concierge panel | Implemented; windowed capture verified |
| WebGL path | No native plugins or filesystem dependency in runtime path | Designed; WebGL build not yet captured |
| Performance | Shared generated materials, low-poly runtime geometry, no per-frame allocation in the main movement loop | Baseline capture pending |
| Blender-to-Unity workflow | Procedural placeholder assets currently document the seam | Next milestone: add one authored Blender asset |
| Testing and maintainability | Focused scripts, API smoke tests, README, this evidence matrix | API tests implemented |

## Honest demo boundary

The API tests prove the local service responses. They do not prove Unity compilation, a WebGL build, or headset behavior. Those should be captured separately and linked here after running the Unity editor/build on the host.

## Interview walkthrough

1. Start `python3 Tools/catalog_server.py`.
2. Open `Assets/Scenes/Main.unity` in Unity 2022.3.22f1.
3. Show movement and dash in the Speed Circuit.
4. Switch to the Shopping Area, inspect a kiosk, and show the JSON-backed product card.
5. Select EXPLORE, STYLE, and TECH to show the recommendation service contract and fallback behavior.
6. Explain the next production step: replace the deterministic ranker with an authenticated backend while preserving the client interface.
