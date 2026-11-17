# Unity Audio Scripts

C# audio scripts I wrote for Unity game projects at Berklee: a shared audio API for a team game, layered music kept in sync on the audio DSP clock, and trigger-driven branching music.

| Script | Project | What it does |
| --- | --- | --- |
| [`AudioBus.cs`](KeepTalking/AudioBus.cs) | Keep Talking, a 4-person team puzzle game | Static audio API the whole team calls: 2D and 3D one-shots routed to an SFX mixer group, and a 0–1 settings slider converted to decibels |
| [`CountdownMusic.cs`](KeepTalking/CountdownMusic.cs) | Keep Talking | A base loop and a tension loop started on the same DSP tick; the tension layer fades in when the countdown drops under 60 seconds |
| [`MusicBranchManager.cs`](BranchingAndLayering/MusicBranchManager.cs) | Mixing, branching and layering project | One looping track per level area, crossfaded in a coroutine with separate fade-in and fade-out times |
| [`MusicZoneTrigger.cs`](BranchingAndLayering/MusicZoneTrigger.cs) | Same project | Box trigger that tells the manager which area the player is in, with an optional return to the start branch on exit |
| [`OneShotTrigger.cs`](BranchingAndLayering/OneShotTrigger.cs) | Same project | Plays a sound when the player enters a trigger, once or every time |

## Highlights

- **Sample-locked layers.** `CountdownMusic` schedules both loops with `AudioSource.PlayScheduled` on the same `AudioSettings.dspTime`, so the silent tension layer is always in time when it fades in.
- **Slider to decibels.** `AudioBus.SetVolume` maps 0–1 to `20 × log10(x)` dB on an exposed mixer parameter, with 0 clamped to the mixer's -80 dB floor.
- **Interruptible crossfades.** `MusicBranchManager` fades every track from its current volume, so turning back mid-fade reverses smoothly.
- **Built for teammates.** One-line calls, warnings instead of exceptions, Inspector tooltips, and editor setup through `Reset()` and a context menu. `CountdownMusic` reads the on-screen timer so a teammate's countdown script needed no changes.

## Using them

Written in Unity 6 (6000.3 and 6000.6).

- **AudioBus** expects an AudioMixer at `Resources/Audio/ZachAudioMixer` with groups `Master/Loops` and `Master/SFX`, and exposed parameters `MasterVolume`, `LoopsVolume` and `SFXVolume`.
- **CountdownMusic** needs AudioBus, a `UIDocument` with a Label named `Countdown` showing `mm:ss`, and loops at `Resources/Audio/Music/music_alert_base_loop` and `music_alert_tension_loop` (or assign your own clips).
- **MusicZoneTrigger** and **OneShotTrigger** add a BoxCollider set to trigger. The player needs the `Player` tag and a Rigidbody or CharacterController.

## What I'd change next

- Pool AudioSources and cap voices in `AudioBus` instead of creating a GameObject per 3D sound.
- Have the timer raise an event instead of `CountdownMusic` parsing label text every frame.
- Start branch tracks with `PlayScheduled` and switch on the next bar instead of mid-phrase.

Zachary Wirth · [zonewrightstudios.com/work](https://zonewrightstudios.com/work)
