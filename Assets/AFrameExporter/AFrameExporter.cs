using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class AFrameExporter : ScriptableObject {

    [HeaderAttribute("General")]
    public string title = "Hello world!";
    public string libraryAddress = "https://aframe.io/releases/latest/aframe.min.js";
    public bool enable_performance_statistics = false;
    [HeaderAttribute("Sky")]
    public bool enable_sky = false;
    public bool sky_color_from_MainCamera_Background = true;
    public Color sky_color = Color.white;
    public Texture sky_texture;
    [HeaderAttribute("Camera")]
    public bool wasd_controls_enabled = true;
    public bool look_controls_enabled = true;
    public bool cursor_visible = true;
    public float cursor_opacity = 1f;
    public float cursor_scale = 1f;
    public Color cursor_color = Color.white;
    public float cursor_offset = 1f;
    public float cursor_maxdistance = 1000f;

    private string indent = "      ";
    private string exporter_path = "Assets/AFrameExporter";
    private string export_path = "Assets/AFrameExporter/export";
    private string export_filename = "index.html";

    //A-FrameをExportする
    public void Export()
    {
        TextAsset template_head = AssetDatabase.LoadAssetAtPath<TextAsset>(exporter_path + "/template_head.txt");
        TextAsset template_append = AssetDatabase.LoadAssetAtPath<TextAsset>(exporter_path + "/template_append.txt");
        TextAsset template_end = AssetDatabase.LoadAssetAtPath<TextAsset>(exporter_path + "/template_end.txt");

        //exportフォルダが無ければ作る
        string guid_exist = AssetDatabase.AssetPathToGUID(export_path);
        if (!Directory.Exists(Application.dataPath + "/AframeExporter/export"))
        {
            AssetDatabase.CreateFolder("Assets/AFrameExporter", "export");
            AssetDatabase.Refresh();
        }

        //imagesフォルダ作る
        guid_exist = AssetDatabase.AssetPathToGUID(export_path + "/images");
        if (!Directory.Exists(Application.dataPath + "/AframeExporter/export/images"))
        {
            AssetDatabase.CreateFolder(export_path, "images");
            AssetDatabase.Refresh();
        }

        //modelsフォルダ作る
        guid_exist = AssetDatabase.AssetPathToGUID(export_path + "/models");
        if (!Directory.Exists(Application.dataPath + "/AframeExporter/export/models"))
        {
            AssetDatabase.CreateFolder(export_path, "models");
            AssetDatabase.Refresh();
        }

        //シーンをコンバート
        string scene_string = convertScene();

        string body_string = template_head.text + scene_string + template_append.text + template_end.text;

        //タイトルなど差し替え
        body_string = body_string.Replace("&TITLE&", title);
        body_string = body_string.Replace("&LIBRARY&", libraryAddress);
        if (enable_performance_statistics)
        {
            body_string = body_string.Replace("a-scene","a-scene stats=\"true\"");
        }

        File.WriteAllText(Application.dataPath + "/AFrameExporter/export/" + export_filename, body_string);
        AssetDatabase.Refresh();
    }

    private string convertScene()
    {
        string ret_str = "";
        bool isThereCamera = false;
        bool isThereLight = false;

        //sky
        if (enable_sky)
        {
            string add_str = "<a-sky ";

            Camera mainCamera = Camera.main;
            if (sky_color_from_MainCamera_Background && mainCamera)
            {
                add_str += "color=\"#" + ColorToHex(mainCamera.backgroundColor) + "\" ";
            }
            else
            {
                add_str += "color=\"#" + ColorToHex(sky_color) + "\" ";
            }

            if (sky_texture)
            {
                string texture_path = AssetDatabase.GetAssetPath(sky_texture);
                string new_path = export_path + "/images/" + Path.GetFileName(texture_path);
                //テクスチャ無ければコピー
                if (AssetDatabase.AssetPathToGUID(new_path) == "")
                {
                    AssetDatabase.CopyAsset(texture_path, new_path);
                }

                add_str += "src=\"images/" + Path.GetFileName(texture_path) + "\" ";
            }

            add_str += "></a-sky>\n";
            ret_str += add_str;
        }

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        int objects_num = allObjects.Length;
        int process_num = 0;
        //オブジェクト全検索
        foreach (GameObject obj in allObjects)
        {
            process_num++;
            //プログレスバー表示
            EditorUtility.DisplayProgressBar("Progress", "Now Processing...   " + process_num + "/" + objects_num, (float)process_num / (float)objects_num);

            // ProjectにあるものならAsset Path, SceneにあるオブジェクトはそのシーンのPathが返ってくる
            string path = AssetDatabase.GetAssetOrScenePath(obj);

            // シーンのPathは拡張子が .unity
            string sceneExtension = ".unity";

            // Path.GetExtension(path) で pathの拡張子を取得
            // Equals(sceneExtension)で sceneExtensionと比較
            bool isExistInScene = Path.GetExtension(path).Equals(sceneExtension);

            // シーン内のオブジェクトかどうか判定
            if (isExistInScene)
            {
                //非表示オブジェクトならスキップ
                if (!obj.activeInHierarchy)
                { continue; }

                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                Light light = obj.GetComponent<Light>();
                SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                SkinnedMeshRenderer skinnedMeshRenderer = obj.GetComponent<SkinnedMeshRenderer>();
                //メッシュフィルターコンポーネントを持ってるなら
                if (meshFilter && meshFilter.sharedMesh)
                {
                    //Cubeの場合
                    if (meshFilter.sharedMesh.name == "Cube")
                    {
                        Vector3 scale = obj.transform.lossyScale;

                        string append_str = indent + "<a-entity geometry=\"primitive: box; width: " + scale.x + "; height: " + scale.y + "; depth: " + scale.z + "\" " + outputRotation(obj) + outputPosition(obj) + outputMaterial(obj) + "></a-entity>\n";
                        ret_str += append_str;
                    }
                    //Sphereの場合
                    else if (meshFilter.sharedMesh.name == "Sphere")
                    {
                        //各軸scaleが使えないので各スケールの平均値をradiusとする
                        Vector3 scale = obj.transform.lossyScale;
                        float radius = 0.5f;
                        if (scale != Vector3.one)
                        {
                            radius = (scale.x + scale.y + scale.z) * 0.333333333f * 0.5f;
                        }
                        string append_str = indent + "<a-entity geometry=\"primitive: sphere; radius: " + radius + "\" " + outputRotation(obj) + outputPosition(obj) + outputMaterial(obj) + "></a-entity>\n";
                        ret_str += append_str;
                    }
                    //Cylinderの場合(Unityのスケール1,1,1のシリンダーは半径0.5で高さ２)  TODO:パックマンみたいに欠けたシリンダー対応 独自コンポーネントくっつけて対応予定
                    else if (meshFilter.sharedMesh.name == "Cylinder")
                    {
                        //例によってスケールのxとzの平均を半径、yを高さとする
                        Vector3 scale = obj.transform.lossyScale;
                        float radius = 0.5f;
                        radius = (scale.x + scale.z) * 0.5f * 0.5f;
                        float height = scale.y * 2f;
                        string append_str = indent + "<a-entity geometry=\"primitive: cylinder; radius: " + radius + "\" height:" + height + "; " + outputRotation(obj) + outputPosition(obj) + outputMaterial(obj) + "></a-entity>\n";
                        ret_str += append_str;
                    }
                    //Planeの場合(Unityのスケール1,1,1のプレーンは幅10高さ10）x90回転でa-frameと揃う
                    else if (meshFilter.sharedMesh.name == "Plane")
                    {
                        Vector3 eulerAngles = obj.transform.eulerAngles;
                        eulerAngles.x -= 90f;
                        float width = obj.transform.lossyScale.x * 10f;
                        float height = obj.transform.lossyScale.z * 10f;
                        string append_str = indent + "<a-entity geometry=\"primitive: plane; width:" + width + "; height:" + height + "\" " + outputRotation(eulerAngles) + outputPosition(obj) + outputMaterial(obj) + "></a-entity>\n";
                        ret_str += append_str;
                    }
                    //TODO:videoの場合
                    //TODO:curvedimageの場合
                    //TODO:videosphereの場合
                    //TODO:Coneの場合
                    //TODO:Ringの場合
                    //TODO:Torusの場合
                    //TODO:Torus Knotの場合
                    //Modelの場合
                    else
                    {
                        string new_path = export_path + "/models/" + meshFilter.sharedMesh.name + ".obj";
                        //obj無ければ作成
                        if (!File.Exists(Application.dataPath + "/AFrameExporter/export/models/" + meshFilter.sharedMesh.name + ".obj"))
                        {
                            ObjExporter.MeshToFile(meshFilter, new_path, true);
                            AssetDatabase.ImportAsset(new_path);
                            var importer = (ModelImporter)ModelImporter.GetAtPath(new_path);
                            importer.animationType = ModelImporterAnimationType.None;
                            AssetDatabase.Refresh();
                        }

                        //マテリアルからテクスチャを取り出す
                        string append_str = indent + "<a-entity loader=\"src: url(models/" + meshFilter.sharedMesh.name + ".obj); format: obj\" " + outputScale(obj) + outputRotation(obj) + outputPosition(obj) + outputMaterial(obj) + "></a-entity>\n";
                        ret_str += append_str;
                    }
                }
                //skinnedMeshModelの場合
                else if (skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh)
                {
                    string new_path = export_path + "/models/" + skinnedMeshRenderer.sharedMesh.name + ".obj";
                    //obj無ければ作成
                    if (!File.Exists(Application.dataPath + "/AFrameExporter/export/models/" + skinnedMeshRenderer.sharedMesh.name + ".obj"))
                    {
                        ObjExporter.SkinnedMeshToFile(skinnedMeshRenderer, new_path, true);
                        AssetDatabase.ImportAsset(new_path);
                        var importer = (ModelImporter)ModelImporter.GetAtPath(new_path);
                        importer.animationType = ModelImporterAnimationType.None;
                        AssetDatabase.Refresh();
                    }

                    //マテリアルからテクスチャを取り出す
                    string append_str = indent + "<a-entity loader=\"src: url(models/" + skinnedMeshRenderer.sharedMesh.name + ".obj); format: obj\" " + outputScale(obj) + outputRotation(obj) + outputPosition(obj) + outputMaterial(obj) + "></a-entity>\n";
                    ret_str += append_str;
                }
                //imageの場合 UnityはQuad シングルスプライトのみ対応
                else if (spriteRenderer && spriteRenderer.sprite && spriteRenderer.sprite.pixelsPerUnit != 0)
                {
                    Sprite sprite = spriteRenderer.sprite;
                    float width = sprite.rect.width / sprite.pixelsPerUnit * obj.transform.lossyScale.x;
                    float height = sprite.rect.height / sprite.pixelsPerUnit * obj.transform.lossyScale.y;

                    //テクスチャ
                    Texture tex = sprite.texture;
                    string tex_str = "";
                    if (tex)
                    {
                        string texture_path = AssetDatabase.GetAssetPath(tex);
                        string new_path = export_path + "/images/" + Path.GetFileName(texture_path);
                        //テクスチャ無ければコピー
                        if (!File.Exists(Application.dataPath + "/AFrameExporter/export/images/" + Path.GetFileName(texture_path)))
                        {
                            AssetDatabase.CopyAsset(texture_path, new_path);
                            AssetDatabase.Refresh();
                        }

                        tex_str = "src=\"images/" + Path.GetFileName(texture_path) + "\" ";
                    }

                    string append_str = indent + "<a-image " + tex_str + "width=\"" + -width + "\" height=\"" + height + "\" " + outputRotation(obj) + outputPosition(obj) + "></a-image>\n";
                    ret_str += append_str;
                }
                //MainCameraの場合
                else if (!isThereCamera && obj.tag == "MainCamera")
                {
                    Camera camera = obj.GetComponent<Camera>();
                    if (camera)
                    {
                        string append_str = indent + "<a-camera " + outputPosition(obj) + " cursor-color=#" + ColorToHex(cursor_color) +
                            " wasd-controls-enabled=" + wasd_controls_enabled.ToString().ToLower() + " fov=" + camera.fieldOfView + " near=" + camera.nearClipPlane + " far=" + camera.farClipPlane +
                            " cursor-maxdistance=" + cursor_maxdistance + " cursor-offset=" + cursor_offset + " cursor-opacity=" + cursor_opacity + " cursor-scale" + cursor_scale +
                            " look-controls-enabled=" + look_controls_enabled.ToString().ToLower() + "></a-camera>\n";
                        ret_str += append_str;
                        isThereCamera = true;
                    }
                }
                //Lightの場合
                else if (light)
                {
                    //DirectionalLightの場合
                    if (light.type == LightType.Directional)
                    {
                        Vector3 forward = -obj.transform.forward;
                        string lightPosition_str = "position=\"" + -forward.x + " " + forward.y + " " + forward.z + "\" ";
                        string append_str = indent + "<a-light type=directional; intensity=" + light.intensity + " color=#" + ColorToHex(light.color) + " " + lightPosition_str + "></a-light>\n";
                        ret_str += append_str;
                        isThereLight = true;
                    }
                    //TODO:PointLightの場合
                    //TODO:SpotLightの場合
                    //TODO:AmbientLightの場合
                    //TODO:Hemisphereの場合
                }
            }
        }

        //Cameraが無い場合はデフォルト設定
        if (!isThereCamera)
        {
            string append_str = indent + "<a-camera cursor-color=#" + ColorToHex(cursor_color) +
                            " wasd-controls-enabled=" + wasd_controls_enabled.ToString().ToLower() + "></a-camera>\n";
            ret_str += append_str;
            ret_str += append_str;
        }

        //Lightが無い場合はデフォルトライト
        if (!isThereLight)
        {
        }

        EditorUtility.ClearProgressBar();

        return ret_str;
    }

    // Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
    string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }

    Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }

    private string outputRotation(GameObject obj)
    {
        Vector3 rotation = obj.transform.rotation.eulerAngles;
        return outputRotation(rotation);
    }

    private string outputRotation(Vector3 eulerAngles)
    {
        if (eulerAngles == Vector3.zero)
        {
            return "";
        }
        return "rotation=\"" + eulerAngles.x + " " + -eulerAngles.y + " " + -eulerAngles.z + "\" ";
    }

    private string outputScale(GameObject obj)
    {
        Vector3 scale = obj.transform.lossyScale;
        return outputScale(scale);
    }

    private string outputScale(Vector3 scale)
    {
        if (scale == Vector3.one)
        {
            return "";
        }
        return "scale=\"" + scale.x + " " + scale.y + " " + scale.z + "\" ";
    }

    private string outputMaterial(GameObject obj)
    {
        string ret_str = "";

        Material mat = obj.GetComponent<Renderer>().sharedMaterial;
        if (!mat)
        {
            return ret_str;
        }

        //Debug.Log(mat.shader.name);
        //シェーダはこれらだけサポート
        if (mat.shader.name == "Standard")
        {
            ret_str = "material=\"";

            //シェーダタイプ
            ret_str += "shader: standard; ";

            //テクスチャ
            ret_str += outputTexture(mat);

            //リピート(xを使う)
            ret_str += "repeat: " + mat.mainTextureScale.x + "; ";

            //カラー
            ret_str += "color: #" + ColorToHex(mat.color) + "; ";

            //メタルネス
            ret_str += "metalness: " + mat.GetFloat("_Metallic") + "; ";

            //スムースネス（roughnessの逆)
            ret_str += "roughness: " + (1f - mat.GetFloat("_Glossiness")) + "; ";

            //透過有効(_Modeが３ならRendering Modeはtransparent)
            ret_str += "transparent: " + (mat.GetFloat("_Mode") == 3 ? "true" : "false") + "; ";

            //透明度
            ret_str += "opacity: " + mat.color.a + "; ";

            if (mat.color.a == 1f)
            {
                //描画面（両面）
                ret_str += "side: double; ";
            }

            //おしまい
            ret_str += "\"";
        }
        else if (mat.shader.name == "Unlit/Color")
        {
            ret_str = "material=\"";

            //シェーダタイプ
            ret_str += "shader: flat; ";

            //描画面（両面）
            ret_str += "side: double; ";

            //おしまい
            ret_str += "\"";
        }
        else if (mat.shader.name == "Unlit/Texture")
        {
            ret_str = "material=\"";

            //シェーダタイプ
            ret_str += "shader: flat; ";

            //テクスチャ
            ret_str += outputTexture(mat);

            //リピート(xを使う)
            ret_str += "repeat: " + mat.mainTextureScale.x + "; ";

            //描画面（両面）
            ret_str += "side: double; ";

            //おしまい
            ret_str += "\"";
        }
        else if (mat.shader.name == "Unlit/Texture Colored")
        {
            ret_str = "material=\"";

            //シェーダタイプ
            ret_str += "shader: flat; ";

            //テクスチャ
            ret_str += outputTexture(mat);

            //リピート(xを使う)
            ret_str += "repeat: " + mat.mainTextureScale.x + "; ";

            //カラー
            ret_str += "color: #" + ColorToHex(mat.color) + "; ";

            //描画面（両面）
            ret_str += "side: double; ";

            //おしまい
            ret_str += "\"";
        }
        else if (mat.shader.name == "Legacy Shaders/Transparent/Diffuse")
        {
            ret_str = "material=\"";

            //シェーダタイプ
            ret_str += "shader: flat; ";

            //テクスチャ
            ret_str += outputTexture(mat);

            //リピート(xを使う)
            ret_str += "repeat: " + mat.mainTextureScale.x + "; ";

            //カラー
            ret_str += "color: #" + ColorToHex(mat.color) + "; ";

            //透過有効(_Modeが３ならRendering Modeはtransparent)
            ret_str += "transparent: true; ";

            //透明度
            ret_str += "opacity: " + mat.color.a + "; ";

            if (mat.color.a == 1f)
            {
                //描画面（両面）
                ret_str += "side: double; ";
            }

            //おしまい
            ret_str += "\"";
        }
        //他のシェーダでもなるべく対応
        else
        {
            ret_str = "material=\"";

            //シェーダタイプ
            ret_str += "shader: standard; ";

            //テクスチャ
            ret_str += outputTexture(mat);

            //リピート(xを使う)
            ret_str += "repeat: " + mat.mainTextureScale.x + "; ";

            if (mat.HasProperty("_Color"))
            {
                //カラー
                ret_str += "color: #" + ColorToHex(mat.color) + "; ";
            }

            //メタルネス
            //ret_str += "metalness: " + mat.GetFloat("_Metallic") + "; ";

            //スムースネス（roughnessの逆)
            //ret_str += "roughness: " + (1f - mat.GetFloat("_Glossiness")) + "; ";

            //透過有効(_Modeが３ならRendering Modeはtransparent)
            //ret_str += "transparent: " + (mat.GetFloat("_Mode") == 3 ? "true" : "false") + "; ";

            //透明度
            //ret_str += "opacity: " + mat.color.a + "; ";

            //描画面（両面）
            ret_str += "side: double; ";

            //おしまい
            ret_str += "\"";
        }

        return ret_str;
    }



    private string outputTexture(Material mat)
    {
        //テクスチャ
        Texture tex = mat.GetTexture("_MainTex");
        if (tex)
        {
            string texture_path = AssetDatabase.GetAssetPath(tex);
            string new_path = export_path + "/images/" + Path.GetFileName(texture_path);
            //テクスチャ無ければコピー
            if (AssetDatabase.AssetPathToGUID(new_path) == "")
            {
                AssetDatabase.CopyAsset(texture_path, new_path);
            }

            return "src: url(images/" + Path.GetFileName(texture_path) + "); ";
        }
        return "";
    }

    private string outputPosition(GameObject obj)
    {
        Vector3 position = obj.transform.position;
        if (position == Vector3.zero)
        {
            return "";
        }
        //return "translate=\"" + -position.x + " " + position.y + " " + position.z + "\" ";
        return "position=\"" + -position.x + " " + position.y + " " + position.z + "\" ";
    }

    //A-Frameを実行する
    public void RunAFrame()
    {
        System.Diagnostics.Process.Start(Application.dataPath + "/AFrameExporter/export/" + export_filename);
    }

    //エクスポートしたA-Frameをクリア
    public void CrearExport()
    {
        string guid_exist = AssetDatabase.AssetPathToGUID(export_path);
        if (guid_exist != "")
        {
            //Directory.Delete(Application.dataPath + "/AFrameExporter/export/", true);
            AssetDatabase.DeleteAsset(export_path);
            AssetDatabase.Refresh();
        }
    }
}
