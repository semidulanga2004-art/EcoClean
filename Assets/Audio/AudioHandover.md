# EcoClean audio handover

Integration base is `player-movement`. Audio changes are on `feature/audio-system`. Production scene integration belongs to the project lead.

## Setup and architecture

Place one `Assets/Prefabs/Audio/AudioManager.prefab` at the root of the first scene that needs audio. The manager persists across scene loads and destroys duplicate manager GameObjects, so keep unrelated components off its root. If testing a production scene directly, place the prefab in an isolated copy or use the audio test scene. The manager has no automatic startup playback. The integration lead decides when music and ambience start and stop.

The prefab creates separate AudioSources for looping music, looping ambience, one-shot effects, and optional victory. Defaults are 0.45 for music, 0.22 for ambience, and 0.75 for effects and victory. The setters clamp to 0–1. Calling either start method again with the currently playing clip leaves playback in place. Missing clips safely do nothing. Repeated victory calls are ignored while the victory source is playing.

The park-wide loop is separate from EcoPark's existing fountain source. Listen to them together before turning on the park-wide loop in gameplay. The manager does not control that source or the beach source.

## Integration calls

After the manager's Awake runs, call its methods from the production integration points below. For example, use `AudioManager.Instance?.PlayCorrectSort();`.

| Game event | Call | Owner and location |
| --- | --- | --- |
| Intro or first music cue | `PlayMusic()` | Project lead, scene entry or flow controller |
| Gameplay starts in the park | `PlayAmbience()` | Project lead, gameplay entry |
| Park exit or music transition | `StopAmbience()`, `StopMusic()` as appropriate | Project lead, scene flow |
| Button click | `PlayUIClick()` | UI owner, button callback |
| Correct bin accepted | `PlayCorrectSort()` | Gameplay owner, confirmed sorting result |
| Incorrect bin rejected | `PlayIncorrectSort()` | Gameplay owner, confirmed sorting result |
| Victory shown | `PlayVictory()` | Project lead, victory transition. Silent until a cleared clip is assigned. |
| User changes a volume | `SetMusicVolume(float)`, `SetAmbienceVolume(float)`, `SetSfxVolume(float)` | UI owner, settings controls |

The existing `PlayMusic(AudioClip)` and `PlaySoundEffect(AudioClip)` calls still work. `PlayAmbience(AudioClip)` accepts an alternate loop. `HasVictoryClip` reports whether a clip is assigned. Leaving a scene does not automatically stop either loop. Stop them explicitly where the transition requires silence.

## Verification

| Status | Check | Evidence |
| --- | --- | --- |
| PASS | Unity 6000.3.27f1 audio compilation, import and references | Batch import in a temporary audio-only project resolved the test scene, prefab, script and five configured clips. |
| PASS | Loop control and volume | Automated Play Mode entered the audio test scene. Both sources reported playing, either could stop independently, and volume inputs outside 0–1 were clamped without changing the other channels. |
| PASS | Effects coexist with loops | Automated Play Mode called the generic and configured effect methods, including repeat calls, and both loop sources kept playing. |
| PASS | Missing and optional clips | Null playback arguments and repeated calls to unassigned victory completed safely. A temporary in-memory test clip exercised the assigned victory path while both loops played. The prefab victory field stayed empty. |
| PASS | Scene transition and duplicate handling | Final automated run called the test scene's own duplicate routine. It reported PASS, and the original manager and playing music survived unloading the original scene. |
| FAIL | Isolated editor Search startup | Unity's internal Search indexer logged an ArgumentOutOfRangeException. The retry excluded that specific editor stack from audio pass/fail and reported no other error or exception during the assertions. |
| PENDING | Full-project interactive Unity run and audible review | Open the test scene in the normal project. Listen for each specific effect, level balance, audible loop seams and continuity through a scene change. The automated run checked playback state, with no human listening. |

### Manual test

1. Open `Assets/Scenes/Audio/AudioManagerPlayModeTest.unity` by itself in Unity 6000.3.27f1. Clear the Console, press Play and open the Game tab.
2. Press **Start music** and **Start ambience**. Press both again while they run. Listen for smooth loops, and use the Music and Ambience sliders one at a time.
3. Press **Play UI click**, **Play correct sort**, and **Play incorrect sort**, including quick repeated clicks. Each cue should be audible and distinct while both loops continue. Turn SFX volume down and back up to confirm the loops are unaffected.
4. Press **Play victory**. Expect the unassigned message and silence from the victory channel.
5. Press **Change scene and try a duplicate**. Expect the PASS message and continued playback. Press **Stop music** and **Stop ambience** separately. Confirm the other channel keeps playing until you stop it.
6. Check the Console for exceptions or missing references. Exit Play Mode to reset the temporary scene.

## Sources and outstanding rights

See [Audio sources and licences](AudioSourcesAndLicences.md) and the [audio inventory](AudioInventory.md). The prefab uses the two OpenGameArt CC0 tracks by cynicmusic and the three Kenney Interface Sounds CC0 cues. Crystal Cave requests credit to pixelsphere.org / The Cynic Project. Enchanted Tiki 86 and the Kenney clips have optional credit.

No victory clip is assigned. Keep the unverified `victory-sound.mp3` and its metadata out of Git. Keep the old Pixabay victory and any unidentified fanfare out of Git. The pre-existing fountain and beach WAVs still have unverified original licences and were left in place.

## Changed files by stage

| Stage | Files |
| --- | --- |
| 2 — Reusable manager | AudioManager script and prefab, isolated scene and helper, their metadata and three parent-folder metadata files |
| 3 — Music and ambience | AudioManager script, prefab, and isolated helper |
| 4 — UI and sorting | AudioManager script, prefab, isolated helper and isolated scene |
| 5 — Victory and balance | AudioManager script, prefab, and isolated helper |
| 6 — Handover | This guide and its Unity metadata |
