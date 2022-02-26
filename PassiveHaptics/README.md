# Project 2 - Passive Haptics (with late extension)

## Functionality:

### Build a simple room
- It is built

### Puzzle
Touching one of the buttons on the ground will "attach" a table to the user's hand. Piching will then set the table's rotation and height. The margins of the table can be adjusted bypinching the spheres and releasing when the desired table size is achieved.
Pushing the button in the middle of the table will confirm the settings and begin the Bug Game.

### Bug Smashing
Bugs will begin to spawn on the table. When it is detected that the user smashed a bug, the bug is flattened and a "splat" sound plays and the score increases.

### Redirected Haptics
Once all of the bugs are smashed, the Redirected Haptics demo will begin. Pinching and placing your wrist on the real-life object will set it's position in the virtual world. Pressing either of the two buttons will randomly select one of the three virutal boxes. When the user reaches for that box, their hand will be redirected towards the real object.

## Extras:
- Use floor for second haptics plane: The inital table calibration needs the user to press a button on the floor
- Allow the user to create a table along any arbitrary axis: My setup allows the user to define a table along any axis
- The Quest also supports the use of hands as an input device: Hands are used as the input device, Pinching is the primary way of interacting with the world.
