![Open Doors (Outer Wilds Mod) Thumbnail](doc/open_doors_thumb.png)

# Open Doors (Outer Wilds Mod) _(WIP)_

This Mod will allow you open any closed doors and other pathways in the Game Outer Wilds, such as closed Nomai doors.

**[Example locations](doc/showcase.md)** for you to try out.

## Controls

The `O` key is the activation key for this mod. You will have to hold it down while pressing the keys below:

|    Key    |                          Action                           |
|:---------:|:---------------------------------------------------------:|
| `O` + `I` |                 Open surrounding pathway                  |
| `O` + `K` |  Open surrounding pathways<br/>(including tractor beams)  |
| `O` + `P` |                Close surrounding pathways                 |
| `O` + `9` |              Reduce door search radius by 10              |
| `O` + `0` |             Increase door search radius by 10             |

It is recommended to close the pathways after using them, otherwise you might stumble on some missing geometry
later on.

## Affected Objects

This currently includes:

- all occurrences of:
    - large orb doors
    - single/both sided rotating orb door
    - tractor beams (toggle active) (use `O` + `K` for this)
    - cacti
    - ghost matter
    - emergency hatches
    - nomai space airlocks
    - stranger two-winged doors (+ those in dream world)
    - stranger secret passage murals (+ those in dream world)
    - stranger elevators (removes entire elevator)
- and some specific places
    - anglerfish overview pod path: stalagmites that block the way
    - lakebed: first maze cave part stalagmites
    - sunless city eye shrine: glass window (messes up gravity direction when passed through)
    - sunless city cannon path: stones and cacti in ghost matter building
    - tower of quantum knowledge: gravity stairs (not visually, only gravity volumes are spawned)
    - tower of quantum knowledge: scout launch holes on middle section
    - old settlement: center piece probe window
    - orbital probe cannon: debris blocking launch module
    - interloper: melting ice on sunward side
    - interloper: specific ghost matter patches
    - quantum moon, giant's deep: north pole tornado
    - quantum moon, dark bramble: north pole brambles (**!** *disables ground collision on entire planet, use
      jetpack* **!**)
    - stranger dam combination house: scout launch hole
    - stranger bell: prisoner vault

Please do not judge the code too harshly, this is my first time writing C# and using Unity. I've had a blast creating
this and I really hope you enjoy using it!
