DRONE SIMULATOR UI ASSET PACK
Source: provided 597x622 reference image.

Files:
- Background_Clean.png: reconstructed background with UI/title areas removed.
- Card_Instructor_Normal.png / Card_Trainee_Normal.png: role selection cards with transparent rounded corners.
- Button_Settings_Normal.png / Controls / About / Quit: bottom buttons with transparent rounded corners.
- Icon_*.png: extracted icon crops.

Unity import:
1. Put PNGs under Assets/UI/DroneSimulator/.
2. Set Texture Type = Sprite (2D and UI).
3. Set Background_Clean as a UI Image with Preserve Aspect.
4. Put cards/buttons on a Canvas using Image components.
5. For clickable cards/buttons, add Button components to the same GameObjects.
6. The supplied assets are the NORMAL visual state. Hover/Pressed states can be generated separately from this reference.
