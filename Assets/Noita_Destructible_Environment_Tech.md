
# Technical Approach Behind *Noita*’s Destructible 2D World

## Pixel-Based Terrain Simulation (Falling-Sand Cellular Automata)
*Noita*’s environment is essentially a **high-resolution cellular automaton** where every pixel is a simulated “particle.” The game adopts a classic *falling sand* approach: each pixel has a material type and follows simple local rules, but the sheer number of pixels yields complex emergent behavior. For example, **liquid pixels** (water, oil, etc.) attempt to move downward each frame; if something solid blocks them, they then try to flow sideways (left or right) into any open space. This creates a rudimentary fluid simulation. Different liquids are given physical densities, so heavier fluids naturally sink below lighter ones – implemented by swapping pixels if a denser liquid sits above a lighter one (allowing, say, oil to float on water). **Gas pixels** (like steam) use an inverted rule: they drift upward if possible, or otherwise spread sideways. **Solid materials** (like soil or stone) generally do not move at all unless destabilized, while **powders** (sand, gunpowder) might behave like heavy “grains” that can fall if not supported. Each pixel’s rule set is relatively simple, but together they produce rich physics-driven behavior.

Critically, the simulation runs **in real time at a pixel resolution**. To maintain performance, the developers chose a fairly low internal resolution for the world – on the order of only a few hundred pixels across the screen. In fact, *Noita*’s internal simulation is roughly ~320×180 pixels for one screen, scaled up for display. This pixelated art style isn’t just aesthetic; it keeps the simulation tractable on typical hardware. The world itself spans many screens, so it consists of tens of thousands of simulated pixels overall. To handle this efficiently, *Noita*’s engine divides the world into a grid of **64×64 pixel chunks**, and it **only updates “dirty” cells** that need simulation. Each chunk maintains a *dirty rectangle* of active pixels, and only those pixels are iterated during the simulation step.

Another important detail is the **update order**: *Noita* updates its cellular automata from the bottom up each frame. This ensures falling materials behave correctly in one pass – e.g. a sand grain can fall multiple steps in the same frame rather than moving down only one pixel per tick. A bottom-up sweep prevents a chain reaction where only the lowest pixels move per frame. Together, these techniques (chunking, dirty-region updates, bottom-up processing) allow the game to simulate a *high-resolution destructible terrain* in real time.

## Parallelization vs. GPU Acceleration
Despite the heavy computation of simulating thousands of pixels, *Noita*’s developers notably **did not use GPU compute shaders** for the physics simulation. Instead, they optimized on the CPU with careful multi-threading. According to Petri Purho of Nolla Games, the team was more comfortable keeping the simulation on the CPU side – debugging and developing gameplay logic entirely in GPU shaders would have been difficult, and shuttling data between CPU and GPU each frame (if the GPU ran the physics) was deemed impractical. Thus, they focused on multi-core CPU optimization. The world is split into 64×64 zones that can be updated in parallel. However, naive multi-threading could lead to conflicts when particles cross chunk boundaries. *Noita* solved this by updating chunks in a **checkerboard pattern across four passes**. In the first pass, every other chunk is updated (so no two adjacent chunks are processed simultaneously). Each thread safely simulates one set of spaced-out chunks, even allowing pixels to move up to ~32 pixels into neighboring regions it “owns” for that pass. Subsequent passes then cover the chunks that were skipped, offsetting the pattern so that all chunks update by the end of the 4th pass. This approach ensures no pixel gets updated twice in one frame and avoids race conditions without heavy locking.

In summary, *Noita* achieves high-performance physics by leveraging CPU multi-threading and spatial partitioning, rather than GPU acceleration.

## Dynamic Material Interactions (Sand, Water, Oil, Gas, Fire, etc.)
A core appeal of *Noita* is the rich **interaction between different material types** in the world. Each material is defined by a set of properties (such as density, flammability, state of matter, etc.) that inform its cellular-automata behavior.

- **Fluids and Gases:** Liquids like water, oil, and lava will flow and mingle according to density. Oil, being less dense than water, will naturally collect on top of water. If oil comes into contact with fire, it will ignite and burn. Water can douse fire and won’t burn – in fact, when fire touches water, *Noita* converts those water pixels into steam (a gas) to simulate evaporation.
- **Fire and Burning:** Fire propagation is modeled stochastically. A pixel on fire may randomly ignite its neighbors. Flammable materials like oil, wood, or coal burn over time, converting to ash or disappearing.
- **Powders and Solids:** Powders behave like granular liquids. Solid materials may remain static unless destabilized by explosions or undercutting.

## Explosions, Destruction and Terrain Modification
Explosions destroy terrain by checking all pixels within a blast radius and deleting or altering them. *Noita* also converts loosened pixels into dynamic particles (sand, debris) and can cut rigid terrain chunks loose to fall under gravity. These falling chunks are treated as physics bodies.

## Collision Detection and Player Interaction
*Noita* bridges its pixel sim and physics engine using **Marching Squares** to outline solid terrain and generate simplified polygon colliders. These are triangulated and updated only when changes occur. Rigid bodies have precomputed pixel layouts, and collisions are managed both ways: physics blocks pixel movement, and pixels define where physics can walk or stand.

## Procedural World Generation (Levels and Seeds)
Worlds are procedurally generated using **Wang tiles** to ensure seamless joins between modular biome chunks. Templates and noise variations combine for level structure and material distribution. Seeds control all randomness and are consistent run-to-run.

## Recreating *Noita*-Style Systems in Unity (URP & Compute Shaders)
You can reproduce this in Unity with:

- **Compute shaders** for cellular automata simulation
- **RenderTextures** for storing material grids
- **URP** for pixel-based rendering and lighting
- **PolygonCollider2D + Marching Squares** for terrain collision
- **Dynamic rigidbodies** to simulate detached terrain chunks
- **Wang tile-based procedural level generation** for controlled structure

## Further Reading & References
- GDC 2020 Talk by Petri Purho: ["Every Pixel Matters"](https://www.youtube.com/watch?v=prXuyMCgbTc)
- Technical breakdown on Noita dev blog: https://noitagame.com
- The Noita Wiki’s PRNG explanation: https://noita.wiki.gg/wiki/Random_Number_Generator
- GPU Compute in Unity Docs: https://docs.unity3d.com/Manual/ComputeShaders.html
