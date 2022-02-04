# Project 1 - Hello Virtual World

## Functionality:

### Interaction
- Objects can be grabbed using the __Hand Trigger__ on the controller
- A baton is available to be picked up. It snaps to the user's hand and can interact with other objects
- Picking up the baton triggers an animation, causing it to expand (_extra_)

### Puzzle
- The puzzle pieces snap to each other (_extra_)
- Once completed, a 3D model of a turtle spawns in
- The piece does not have to be the "correct" alignment to snap... (_extra_)
- ...However the puzzle will only be completed and the turtle will spawn if all of the pieces are "correct" 

### Teleportation
- Holding the __A__/__X__ button shines a laser though the scene
- If the teleport is invalid, the laser will turn red (_extra_)
- Releasing the pressed button teleports the user to the correct location at the correct height and offset
- A preview of what the user's view will be once teleported appears on the user's other hand (_extra_)
- Pressing the other button (__X__ if __A__ is held, __A__ if __X__ is held) cancels the teleport (_extra_)

### Radial spawning menu (_extra_)
- Holding down one of the __Index Triggers__ will open a radial menu where objects can be spawned in
- Using the __Thumbstick__ will select one of the objects
- Relasing the __Index Trigger__ will spawn the object
- Puzzle pieces spawned from the menu will work with pieces already in the scene
