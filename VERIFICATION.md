# Mall Demo verification

This file records the evidence behind the claims in the portfolio README.

## Verified

- Unity editor version: `2022.3.22f1`
- Unity script compilation and scene generation: passed through `ProjectBuilder.BuildProject`
- Standalone Linux player build: passed through `ProjectBuilder.BuildStandalone`
- Player artifact: `Builds/Linux/MallDemo.x86_64` (generated; ignored by Git)
- Catalog API tests: `3/3` passed with `python3 -m unittest discover -s Tools -p 'test_*.py' -v`
- Windowed demo capture: `demo/MallDemo-demo.mp4`, H.264, `1024×768`, `20` seconds (generated; ignored by Git)
- Generic-name scan: no project files contain the former company or character names
- MIT License: copyright assigned to Christian Busca

## Repeatable commands

```bash
python3 -m unittest discover -s Tools -p 'test_*.py' -v
```

Open the project in Unity 2022.3.22f1 and use the editor methods documented in `Assets/Editor/ProjectBuilder.cs` to regenerate the scene or player. The windowed capture can be recreated with:

```bash
Tools/record_demo.sh demo/MallDemo-demo.mp4 20
```

## Boundaries

- The catalog and recommendation service are deterministic local demo endpoints with an offline fallback; they are not a production AI backend.
- A WebGL build has not yet been captured.
- A Unity Profiler baseline has not yet been recorded.
- The generated video and standalone build are release artifacts, not source files for the Git commit.
