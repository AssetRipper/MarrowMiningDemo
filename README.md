# Marrow Mining Demo

This project demonstrates how the [AssetRipper.Mining.PredefinedAssets](https://www.nuget.org/packages/AssetRipper.Mining.PredefinedAssets) package can be used to generate package data json files for the [premium edition](https://www.patreon.com/ds5678) of [AssetRipper](https://github.com/AssetRipper/AssetRipper).

The package data enables AssetRipper to correctly output references to the package's assets and avoid duplicating those assets into the exported project.

## Source Files

For this demonstration, I have used the [Marrow SDK](https://github.com/StressLevelZero/MarrowSDK) from [Stress Level Zero](https://www.stresslevelzero.com/), which is MIT licensed.

I have also used [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity). It's an amazing, MIT licensed, project that lets game developers use NuGet packages directly in the Unity Editor.

## Requirements

* .NET Standard - I cannot guarantee that this datamining process would work if the Unity project is targeting .NET Framework.
  * The Marrow SDK targets .NET Framework, but I switched it over to .NET Standard.
  * This .NET Standard requirement is only in regards to this datamining project. Projects exported with AssetRipper can utilize any target.
* `2019.3` or newer - As far as I know, this is when the package manager was introduced.
  * Additional restrictions may be caused by the addition and removal of various Unity APIs. This project was developed on `2021.3.5f1`.

## Subset

The implementation provided here does not cover all asset types. Notable exclusions are prefabs, scenes, and scriptable objects.

## Output

In the [releases](https://github.com/AssetRipper/MarrowMiningDemo/releases) tab, I have added a zip archive of the output from the project. The output includes data for all packages in the project, including ones not necessarily related to the SDK, such as [TextMeshPro](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html).
