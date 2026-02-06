P.S.1 This assignment was completed individually, and the group members were not involved. The Git repository link was provided solely to make it easier for the reviewer to inspect the specific changes.

P.S.2 Because of time constraints, not all features were migrated. The overall process involved replacing the SDL2 library with SDL3 and then resolving all resulting errors. The game can now run and be tested normally.


## SDL3 Upgrade (Shard Engine 1.4.0 – Dandelion)

This engine has been migrated from SDL2 to SDL3 using ppy.SDL3-CS bindings.

### Completed
- Core SDL initialization (window, renderer)
- Rendering pipeline updated to SDL3 (`SDL_RenderTexture`, `SDL_GetTextureSize`)
- Event handling updated to SDL3 event system
- SDL_image migrated to SDL3_image
- SDL_ttf migrated at initialization level

### Known limitations / TODO
- Text rendering temporarily disabled due to major SDL3_ttf API changes
- Audio playback (SoundBeep) stubbed during migration; requires SDL3 audio stream refactor
- Tested primarily on Windows (engine target platform)

This upgrade focuses on engine stability and SDL3 compatibility rather than feature completeness.
