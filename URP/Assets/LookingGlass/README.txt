The LookingGlass Unity Plugin
v2.0.0-alpha13

Please visit the docs site for explanations of how everything works:
https://docs.lookingglassfactory.com/developer-tools/unity

--- --- ---

**WARNING:** We recently migrated to using AssemblyDefinitions, so your code might temporarily break when Unity attempts to compile your project.

To fix this, select your project's AssemblyDefinition asset(s), and make sure to reference the LookingGlass and/or LookingGlass.Editor assemblies, and hit "Apply" in the inspector.

If you're receiving compile-time errors regarding being unable to find certain symbols, it may be because we had some scripts in the global namespace that were moved into LookingGlass and LookingGlass.Editor.

To fix this, add "using LookingGlass;" or "using LookingGlass.Editor;" at the top of your C# scripts where needed/

--- --- ---

After you're set up, the example scenes in Assets/LookingGlass/Examples is a good way to learn about the features of the plugin.
