# UnityAFrameExporter
Export A-Frame From Unity Scene.
## Use Sample
1. Clone This Repository in your PC.<br><br>
2. Open this Repository as Project from Unity3D.<br><br>
3. Double click sample to open sample Scene.<br><br>
![image1](https://raw.github.com/wiki/umiyuki/UnityAFrameExporter/AFrame1.jpg)
4. Click AFrameExporter prefab to show exporter inspector.<br><br>
![image1](https://raw.github.com/wiki/umiyuki/UnityAFrameExporter/AFrame2.jpg)
5. Click Export button on inspector.<br>
  Export Scene as A-Frame in created export folder.<br><br>
6. Click Run button to show created A-Frame on browser.<br><br>
![image1](https://raw.github.com/wiki/umiyuki/UnityAFrameExporter/AFrame3.jpg)

## Import to your Unity3D Project.
1. Clone This Repository in your PC.<br><br>
2. Copy "AFrameExporter" and "CombineMeshes" Folder to your project from repository<br><br>
3. Open your project from Unity3D.<br><br>
4. Open scene you want to export.<br><br>
5. Click AFrameExporter prefab to show exporter inspector.<br><br>
6. Click Export button on inspector.<br><br>
7. Click Run button to show created A-Frame on browser.<br><br>

## Export Option
![image1](https://raw.github.com/wiki/umiyuki/UnityAFrameExporter/AFrame4.jpg)
<br>**◆General**<br>  
*・Title*<br>
  Title of A-Frame.<br>  
*・Library Address*<br>
  A-Frame library address you want to use.<br>    
*・Enable_performance_statistics*  
  Show Performance Statistics.<br>    
**◆Sky**<br>  
*・Enable_Sky*  
  Enable A-Frame Sky.<br>    
*・Sky_color_from_Main Camera_Background*  
  Use sky color from Main Camera Background.<br>    
*・Sky_color*  
  Sky color.<br>    
*・Sky_texture*
  Sky texture.<br>    
**◆Camera**<br>  
*・Wasd_controls_enabled*  
  Enable WASD control.<br>    
*・Look_controls_enabled*  
  Enable Look control.<br>    
*・Enable_Sky*  
  Enable A-Frame Sky.<br>    
*・Cursor_visible*  
  Change cursor visible.<br>    
*・Cursor_opacity*  
  Change cursor opacity.0 to 1.<br>    
*・Cursor_scale*  
  Change cursor scale.<br>    
*・Cursor_color*  
  Change cursor color.<br>    
*・Cursor_offset*  
  Change cursor offset.<br>    
*・Cursor_maxdistance*  
  Change cursor max distance.<br>    
**◆Clear Exported Files**<br>  
  Clean exported files.  
  If you editted export folder. These file will be deleted.<br>

## Caution

・You can't use Asset from Unity Asset Store limited of EULA.  
  But, you can ask asset developer about it.<br>    
・If you editted index.html in export folder, and re-export , It will be deleted.  
  Before edit A-Frame, please copy export folder to another directory.<br>  
・Export only main texture from object.

## Supported Shader Type
・Standard  
・Unlit/Color  
・Unlit/Texture  
・Unlit/Texture Colored  
・Legacy Shaders/Transparent/Diffuse    

## Supported Unity3D Objects
**・Main Camera**    
Supported parameters are Position, Rotation, Fov, NearClip, FarClip.<br>  
**・Light**  
Supported only directional light.  
Supported parameters are Position, Rotaion, Intensity, Color.<br>  
**・Single Sprite**  
Export as Image.<br>  
**・Cube**  
Export as Box.  
Supported parameters are Scale xyz.<br>  
**・Sphere**  
Export as Sphere.  
Scale parameters are exported average xyz. because A-Frame Sphere have parameter only radius.<br>  
**・Cylinder**  
Export as cylinder.  
Scale y export as height.  
Scale xz are exported average for A-Frame cylinder radius.<br>  
**・Plane**  
Export as plane.<br>  
**・Other Meshes**  
Export as Obj.  
