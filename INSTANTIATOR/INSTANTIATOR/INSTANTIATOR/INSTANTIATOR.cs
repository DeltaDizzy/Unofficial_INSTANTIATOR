/*
 Created by Artyomka15 on the KSP forums
 1.3 port and additional editing done by The White Guardian on the KSP forums
 Commits and implemented suggestions will also be credited here
*/

using UnityEngine;

namespace INSTANTIATOR
{
    public struct ScaledObject
    {
        public string name;
        public string body;
        public Vector3 scale;
        public Shader shader;
        public Quaternion rotation;
        public bool invertNormals;
        public Texture2D tex;
        public string type;
        public bool ignoreLight;
        public ScaledObject(string n, string b, string s, string sh, string r, string inv, string tex, string type, string ignoreLight)
        {
            name = n; 
            body = b; 
            scale = ConfigNode.ParseVector3(s); 
            shader = Shader.Find(sh); 
            rotation = Quaternion.Euler(ConfigNode.ParseVector3(r)); 
            invertNormals = bool.Parse(inv); 
            this.tex = GameDatabase.Instance.GetTexture(tex, false); 
            this.type = type;
            this.ignoreLight = bool.Parse(ignoreLight);
        }

        //From the original INSTANTIATOR code by Artyomka15
        static void InvertNormals(MeshFilter filter)
        {
            //Grab the mesh
            Mesh mesh = filter.mesh;

            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++) { normals[i] = -normals[i]; }
            mesh.normals = normals;

            for (int x = 0; x < mesh.subMeshCount; x++)
            {
                int[] tris = mesh.GetTriangles(x);
                for (int y = 0; y < tris.Length; y += 3)
                {
                    int temp = tris[y];
                    tris[y] = tris[y + 1];
                    tris[y + 1] = temp;
                }
                mesh.SetTriangles(tris, x);
            }
        }

        /// <summary>
        /// Builds a ScaledObject
        /// </summary>
        internal void Build()
        {
            Debug.Log("[INSTANTIATOR_ScaledObject]: Building object [" + name + "].");
            CelestialBody targetBody = FlightGlobals.GetBodyByName(body);
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            if (type == "Billboard")
            {
                obj.transform.SetParent(targetBody.scaledBody.GetComponentInChildren<SunCoronas>().transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = scale;
                obj.GetComponent<Renderer>().material.shader = shader;
                obj.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
                obj.layer = targetBody.scaledBody.layer;
                obj.name = name;
            }
            else
            {
                obj.transform.SetParent(targetBody.scaledBody.transform);
                if (type == "Sphere")
                {
                    obj.GetComponent<MeshFilter>().mesh = FlightGlobals.GetBodyByName("Sun").scaledBody.GetComponentInChildren<MeshFilter>().mesh;
                }
                obj.GetComponent<MeshFilter>().mesh.RecalculateBounds();
                obj.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = scale;
                obj.GetComponent<Renderer>().material.shader = shader;
                obj.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
                obj.transform.rotation = rotation;
                if (ignoreLight) obj.layer = 9; else obj.layer = 10;
                obj.name = name;
                if (invertNormals) InvertNormals(obj.GetComponent<MeshFilter>());
            }
        }
    }

    //Startup on planetary system spawn
    [KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class INSTANTIATOR : MonoBehaviour
    {
        /// <summary>
        /// Stores all found INSTANTIATOR config nodes
        /// </summary>
        UrlDir.UrlConfig[] foundObjects;
        

        private void Start()
        {

            System.Collections.Generic.List<ScaledObject> objList = new System.Collections.Generic.List<ScaledObject>();
            
            foundObjects = GameDatabase.Instance.GetConfigs("INSTANTIATOR"); //Grab all INSTANTIATOR nodes

            //If we found no objects, we quit.
            if (foundObjects.Length < 1)
            {
                Debug.LogWarning("[INSTANTIATOR]: No config files found. Shutting down.");
                return;
            }

            //Process all INSTANTIATOR nodes
            foreach(UrlDir.UrlConfig config in foundObjects)
            {
                //In each INSTANTIATOR node, index all SCALED_OBJECT nodes
                foreach(ConfigNode ObjNode in config.config.GetNodes("SCALED_OBJECT"))
                {
                    //Make a new entry
                    objList.Add(new ScaledObject(ObjNode.GetValue("Name"),ObjNode.GetValue("Body"), ObjNode.GetValue("Scale"), ObjNode.GetValue("Shader"), ObjNode.GetValue("Rotation"), ObjNode.GetValue("InvertNormals"), ObjNode.GetValue("Texture"), ObjNode.GetValue("Type"), ObjNode.GetValue("IgnoreLight")));
                }
            }

            //Now, we execute
            foreach(ScaledObject o in objList)
            {
                o.Build();
            }
        }

        static void Log(string msg)
        {
            Debug.Log("[INSTANTIATOR]: " + msg);
        }
        

        







        /*
        /// <summary>
        /// Prodcesses a SCALED_OBJECT node
        /// </summary>
        /// <param name="node">The SCALED_OBJECT node to be edited</param>
        void ProcessObjNode(ConfigNode node)
        {
            Log("Processing SCALED_OBJECT node named [" + node.GetValue("Name") + "].");

            CelestialBody targetBody = FlightGlobals.GetBodyByName(node.GetValue("Body"));

            string objType = node.GetValue("Type");

            if (objType == "Billboard")
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.SetParent(targetBody.scaledBody.GetComponentInChildren<SunCoronas>().transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = ConfigNode.ParseVector3(node.GetValue("Scale"));
                ProcessShader(obj, node);
                obj.transform.rotation = Quaternion.Euler(ConfigNode.ParseVector3(node.GetValue("Rotation")));
                obj.layer = targetBody.scaledBody.layer;

                obj.name = node.GetValue("Name");
            }
            else
            {
                GameObject obj;
                if(objType != "Plane")
                {
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                }
                else
                {
                    obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                }
                obj.transform.SetParent(targetBody.scaledBody.transform);
                if (objType == "Sphere")
                {
                    obj.GetComponent<MeshFilter>().mesh = FlightGlobals.GetBodyByName("Sun").scaledBody.GetComponentInChildren<MeshFilter>().mesh;
                }
                obj.GetComponent<MeshFilter>().mesh.RecalculateBounds();
                obj.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = ConfigNode.ParseVector3(node.GetValue("Scale"));
                ProcessShader(obj, node);
                obj.transform.rotation = Quaternion.Euler(ConfigNode.ParseVector3(node.GetValue("Rotation")));
                if (bool.Parse(node.GetValue("IgnoreLightCast")))
                {
                    obj.layer = 9;
                }
                else
                {
                    obj.layer = 10;
                }
                obj.name = node.GetValue("Name");
                if (bool.Parse(node.GetValue("InvertNormals")))
                {
                    InvertNormals(obj.GetComponent<MeshFilter>());
                }
            }
        }

        

        const string basePath = "INSTANTIATOR/";
        void ProcessShader(GameObject obj, ConfigNode node)
        {
            string targetShader = node.GetValue("Shader");
            bool doubleMat = bool.Parse(node.GetValue("DoubleSidedMaterial"));
            Material mat;
            ConfigNode materialNode = node.GetNode("Material");
            Vector2 dScale = ConfigNode.ParseVector2(node.GetValue("Detail_Scale"));
            if (targetShader != "Unlit")
            {
                if (targetShader == "Basic")
                {
                    if(doubleMat)
                    {
                        mat = new Material(ShaderLoader.GetShader(basePath + "Basic_Double"));
                    }
                    else
                    {
                        mat = new Material(ShaderLoader.GetShader(basePath + "Basic_Single"));
                    }
                    mat.SetTexture("_Emission", GameDatabase.Instance.GetTexture(materialNode.GetValue("EmissionTex"), false));
                    mat.SetTexture("_DetailEmi", GameDatabase.Instance.GetTexture(materialNode.GetValue("Detail_Emission"), false));
                }
                else
                {
                    //We use PBR
                    if(doubleMat)
                    {
                        mat = new Material(ShaderLoader.GetShader(basePath + "PBR_Double"));
                    }
                    else
                    {
                        mat = new Material(ShaderLoader.GetShader(basePath + "PBR_Single"));
                    }
                    mat.SetTexture("_EffectTex", GameDatabase.Instance.GetTexture(materialNode.GetValue("Effect_Texture"), false));
                    mat.SetTexture("_DetailEffect", GameDatabase.Instance.GetTexture(materialNode.GetValue("Detail_Effect_Texture"), false));
                    mat.SetTextureScale("_DetailEffect", dScale);
                }
                mat.SetTexture("_BumpMap", GameDatabase.Instance.GetTexture(materialNode.GetValue("BumpMap"), true));
                mat.SetTexture("_DetailNrm", GameDatabase.Instance.GetTexture(materialNode.GetValue("DetailBump"), true));
                mat.SetTextureScale("_DetailNrm", dScale);
                
                mat.SetTextureScale("_DetailEmi", dScale);
                mat.SetColor("_Emi_Col", ConfigNode.ParseColor(materialNode.GetValue("Emission_Color")));
            }
            else
            {
                if (doubleMat)
                {
                    mat = new Material(ShaderLoader.GetShader(basePath + "Unlit_Double"));
                }
                else
                {
                    mat = new Material(ShaderLoader.GetShader(basePath + "Unlit_Single"));
                }
            }
            mat.SetColor("_Color", ConfigNode.ParseColor(materialNode.GetValue("Color")));
            mat.SetTexture("_MainTex", GameDatabase.Instance.GetTexture(materialNode.GetValue("Texture"), false));
            mat.SetTexture("_DetailTex", GameDatabase.Instance.GetTexture(materialNode.GetValue("Detail_Texture"), false));
            mat.SetTextureScale("_DetailTex", dScale);
            mat.SetFloat("_DetailDistance", float.Parse(materialNode.GetValue("Detail_Distance")));
            mat.SetTexture("_DetailMask", GameDatabase.Instance.GetTexture(materialNode.GetValue("Detail_Mask"), false));
            mat.SetFloat("_FadeEnabled", BoolToFloat(bool.Parse(materialNode.GetValue("Fade_Enabled"))));
            obj.GetComponent<Renderer>().material = mat;
        }

        static float BoolToFloat(bool b)
        {
            if (b) return 1f;
            else return 0f;
        }

        /*
         * Code for editing all properties of a material
        public enum ParseMode
        {
            Scale,
            Offset,
            Texture,
            BumpMap,
            Color,
            Float,
            Int
        }
        struct Pass
        {
            public string valueName, valueToBeAssigned;
            public ParseMode mode;
            public Pass(string name, string value, ParseMode mode)
            {
                valueName = name;
                valueToBeAssigned = value;
                this.mode = mode;
            }
            public static Pass LoadFromNode(string input)
            {
                //Setup the separator
                System.Collections.Generic.List<char> splitter = new System.Collections.Generic.List<char>();
                splitter.Add(';');
                string[] parts = input.Split(splitter.ToArray(), 3); //We need 3 parts
                Pass output = new Pass(); //For returning
                //Check if something is missing
                if (parts.Length < 3)
                {
                    return output; //Return output while it's empty
                }
                
                output.valueName = parts[0];
                output.valueToBeAssigned = parts[1];
                if(!System.Enum.TryParse<ParseMode>(parts[3], out output.mode))
                {
                    throw new System.InvalidCastException("Could not parse ParseMode! [" + parts[3] + "] is invalid!");
                }
                return output;
            }
        }
        Material LoadNode(ConfigNode n)
        {
            ConfigNode ShaderNode = n.GetNode("Shader"); //Grab the config node storing all material passes and the shader name
            Shader s = ShaderLoader.GetShader(ShaderNode.GetValue("Shader")); //Find the shader
            if(s = null)
            {
                //Check if shader was not found, abort if we have no shader.
                return null;
            }
            Material mat = new Material(s); //Create a new material, using the found shader as a base.
            string[] values = ShaderNode.GetValues("Pass"); //Each pass is an edit to the material
            System.Collections.Generic.List<Pass> edits = new System.Collections.Generic.List<Pass>(); //Stores all edits
            //Format: Pass = string ValueToBeEdited;string valueToBeAssigned;ParseMode typeOfEditedValue
            //We use the semicolon so we can parse colors, vectors and filepaths correctly
            foreach(string pass in values)
            {
                edits.Add(Pass.LoadFromNode(pass));
            }

            //Now we have all passes stored. Time to apply them.
            foreach(Pass p in edits)
            {
                switch(p.mode)
                {
                    case ParseMode.BumpMap:
                        mat.SetTexture(p.valueName, GameDatabase.Instance.GetTexture(p.valueToBeAssigned, true));
                        break;
                    case ParseMode.Color:
                        mat.SetColor(p.valueName, ConfigNode.ParseColor(p.valueToBeAssigned));
                        break;
                    case ParseMode.Float:
                        mat.SetFloat(p.valueName, float.Parse(p.valueToBeAssigned));
                        break;
                    case ParseMode.Int:
                        mat.SetInt(p.valueName, int.Parse(p.valueToBeAssigned));
                        break;
                    case ParseMode.Offset:
                        mat.SetTextureOffset(p.valueName, ConfigNode.ParseVector2(p.valueToBeAssigned));
                        break;
                    case ParseMode.Scale:
                        mat.SetTextureScale(p.valueName, ConfigNode.ParseVector2(p.valueToBeAssigned));
                        break;
                    case ParseMode.Texture:
                        mat.SetTexture(p.valueName, GameDatabase.Instance.GetTexture(p.valueToBeAssigned, false));
                        break;
                }
            }
            return mat;
        }
        */
    }
}











































/*﻿
//Grab Unity
using UnityEngine;
using System.Collections;
using INSTANTIATOR.Load;

namespace INSTANTIATOR
{
    //Grab KSP
    [KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class ConfigLoader : MonoBehaviour //Monobehaviour script.
    {
        //This will store the config nodes
        ConfigNode ObjectSettings;

        //Start loads on startup, perfect for a one-time object insertion.
        void Start()
        {
            //Grab all external data
            ShaderLoader.LoadAssetBundle("INSTANTIATOR/Shaders", "INSTANTIATOR_Shaders");

            //New method (thank you, Thomas P!)
            ObjectSettings = GameDatabase.Instance.GetConfigs("INSTANTIATOR")[0].config;

            //Legacy config grab
            //ObjectSettings = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/INSTANTIATOR/Objects.cfg");
            
            //Now read the config
            
            ReadConfiguration();
            
        }
        
        //Move to new void, possible future in-game editing by re-calling the void?
        void ReadConfiguration()
        {
            Debug.Log("[INSTANTIATOR]: Loading config files...");
            //Search through the config for a SCALED_OBJECT node
            foreach (ConfigNode node in ObjectSettings.GetNodes("SCALED_OBJECT"))
            {
                //Give the GameObject a unique name for Unity's sake
                string name = node.GetValue("name");
                Debug.Log("[INSTANTIATOR]: Found a SCALED_OBJECT node with name [" + name + "]. Patching.");
                //The type of object to be spawned
                string type = node.GetValue("type");
                //The scale of the ring
                Vector3 dimensions = ConfigNode.ParseVector3(node.GetValue("scale"));
                //The target shader
                string shader = node.GetValue("shader");
                //The main texture
                string tex = node.GetValue("main_tex");
                //The scale of the main texture
                Vector2 scale = ConfigNode.ParseVector2(node.GetValue("main_tex_scale"));
                //The offset of the main texture
                Vector2 offset = ConfigNode.ParseVector2(node.GetValue("main_tex_offset"));
                //The rotation of the object in 3D space
                Quaternion rot = Quaternion.Euler(ConfigNode.ParseVector3(node.GetValue("rotation")));
                //The name of the celestial body that the object should orbit
                string bodyName = node.GetValue("bodyName");
                //If the normals should be inverted (ergo draw the textures on the outside or the inside of the object)
                bool InvertNormals = bool.Parse(node.GetValue("invertNormals"));

                bool IgnoreLight = bool.Parse(node.GetValue("ignoreLight"));
                //Call the function that places the object
                PlaceObject(name, type, dimensions, shader, tex, rot, bodyName, InvertNormals, scale, offset, IgnoreLight);
            }

            //Search through the config for a TWG node
            foreach (ConfigNode node in ObjectSettings.GetNodes("TWG_OBJECT"))
            {
                //Object name
                string name = node.GetValue("name");
                Debug.Log("[INSTANTIATOR]: Found a TWG_OBJECT node with name [" + name + "]. Patching.");
                //Object scale
                Vector3 scale = ConfigNode.ParseVector3(node.GetValue("scale"));
                //Object type
                string type = node.GetValue("type");
                //Parent body
                string parentBody = node.GetValue("parent");
                //Rotation of the object
                Quaternion rot = Quaternion.Euler(ConfigNode.ParseVector3(node.GetValue("rotation")));
                //Main texture
                string path = node.GetValue("assetPath");
                //Main color (the texture is multiplied by this color, so use it to recolor)
                Vector4 colVec = ConfigNode.ParseVector4(node.GetValue("color"));
                //Convert vector4 to color while accounting for the 255-style format used by image editing software
                Color col = new Color(colVec.x / 255, colVec.y / 255, colVec.z / 255, colVec.w / 255);
                //Detail scale and offset. Basically tiles and offsets the texture for maximum control.
                Vector4 detailSettings = ConfigNode.ParseVector4(node.GetValue("detailSettings"));
                //Format: 'detailSettings = scale x, scale y, offset x, offset y'
                float shininess = float.Parse(node.GetValue("shininess"));
                float specular = float.Parse(node.GetValue("specular"));

                //Place the object
                Finalize(name, scale, type, parentBody, rot, path, col, shininess, specular, detailSettings);
            }
            
        }

        public static bool ORGate(bool a, bool b)
        {
            if (!a && !b)
            {
                return false;
            }
            else return true;
        }

        //Actually place the planets
        public static void Finalize(string n, Vector3 s, string type, string parent, Quaternion rotation, string assetPath, Color mC, float Shiny, float Specular, Vector4 settings)
        {
            

            //Target a celestial
            CelestialBody Target = FlightGlobals.GetBodyByName(parent);

            //Create a new GameObject.
            GameObject createType(string objType)
            {
                if (ORGate(objType == "Cube", objType == "cube"))
                {
                    return GameObject.CreatePrimitive(PrimitiveType.Cube);
                }
                else if (ORGate(objType == "Sphere", objType == "sphere"))
                {
                    GameObject o = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    o.GetComponent<MeshFilter>().mesh = FlightGlobals.GetBodyByName("Sun").scaledBody.GetComponentInChildren<MeshFilter>().mesh;
                    return o;
                }
                else if (ORGate(objType == "Plane", objType == "plane"))
                {
                    return GameObject.CreatePrimitive(PrimitiveType.Plane);
                }
                else return GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
            GameObject obj = createType(type);


            //Parent it
            obj.transform.SetParent(Target.scaledBody.transform);

            //Recalculate
            obj.GetComponent<MeshFilter>().mesh.RecalculateBounds();
            obj.GetComponent<MeshFilter>().mesh.RecalculateNormals();

            //Reposition
            obj.GetComponent<Transform>().localPosition = Vector3.zero;

            //Rotate
            obj.GetComponent<Transform>().rotation = rotation;
            
            //Scale it up!
            obj.GetComponent<Transform>().localScale = s;

            //Shader
            obj.GetComponent<Renderer>().material.shader = ShaderLoader.GetShader("Ring");

            //Load textures from the asset folder
            Texture loadFromBundle(string name, bool asNormalMap)
            {
                if (GameDatabase.Instance.GetTexture(assetPath + "/" + name, asNormalMap) == null)
                {
                    Debug.Log("[INSTANTIATOR]: Could not find the [" + name + "] texture. Loading default texture instead.");
                    return SelectBuiltin(name);
                }
                else
                {
                    Debug.Log("[INSTANTIATOR]: Loading texture [" + name + "] from asset folder [" + assetPath + "].");
                    return GameDatabase.Instance.GetTexture(assetPath + "/" + name, asNormalMap);
                }
            }

            //Select a standard INSTANTIATOR texture
            Texture SelectBuiltin(string t)
            {
                if (t == "Main")
                {
                    return GameDatabase.Instance.GetTexture("INSTANTIATOR/Data/1", false);
                    
                }
                else if (t == "Normal")
                {
                    return GameDatabase.Instance.GetTexture("INSTANTIATOR/Data/3", true);
                }
                else if (t == "Emission")
                {
                    return GameDatabase.Instance.GetTexture("INSTANTIATOR/Data/1", false);
                }
                else if (t == "Transmission")
                {
                    return GameDatabase.Instance.GetTexture("INSTANTIATOR/Data/2", false);
                }
                else return GameDatabase.Instance.GetTexture("INSTANTIATOR/Data/4", false);
            }
            //Set all of the shader values
            obj.GetComponent<Renderer>().material.SetTexture("_MainTex", loadFromBundle("Main", false));
            obj.GetComponent<Renderer>().material.SetTexture("_BumpMap", loadFromBundle("Normal", true));
            obj.GetComponent<Renderer>().material.SetFloat("_Specular", Specular);
            obj.GetComponent<Renderer>().material.SetFloat("_Shininess", Shiny);
            obj.GetComponent<Renderer>().material.SetTexture("_Emission", loadFromBundle("Emission", false));
            obj.GetComponent<Renderer>().material.SetTexture("_Transmission", loadFromBundle("Transmission", false));
            obj.GetComponent<Renderer>().material.SetTexture("_Detail", loadFromBundle("Detail", false));
            obj.GetComponent<Renderer>().material.SetTextureScale("_Detail", new Vector2(settings.x, settings.y));
            obj.GetComponent<Renderer>().material.SetTextureOffset("_Detail", new Vector2(settings.z, settings.w));

        }
        
        

        //For the 'SCALED_OBJECT' nodes
        public static void PlaceObject(string name, string objectType, Vector3 dimensions, string shader, string texturePath, Quaternion rotation, string getBodyByName, bool invert, Vector2 scale, Vector2 offset, bool ignore)
        {
            //Code from the original INSTANTIATOR by Artyomka15
            CelestialBody tgt = FlightGlobals.GetBodyByName(getBodyByName);

            if (objectType != "billboard")
            {

                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);



                obj.transform.SetParent(tgt.scaledBody.transform);

                if (objectType == "sphere")
                {
                    obj.GetComponent<MeshFilter>().mesh = FlightGlobals.GetBodyByName("Sun").scaledBody.GetComponentInChildren<MeshFilter>().mesh;
                }
                obj.GetComponent<MeshFilter>().mesh.RecalculateBounds();
                obj.GetComponent<MeshFilter>().mesh.RecalculateNormals();

                obj.GetComponent<Transform>().localPosition = Vector3.zero;
                obj.GetComponent<Transform>().localScale = dimensions;
                obj.GetComponent<Renderer>().material.shader = Shader.Find(shader);
                obj.GetComponent<Renderer>().material.SetTexture("_MainTex", GameDatabase.Instance.GetTexture(texturePath, false));
                obj.GetComponent<Transform>().rotation = rotation;
                if (ignore) { obj.layer = 9; }
                else { obj.layer = 10; }



                obj.name = name;

                if (invert)
                {
                    InvertNormals(obj.GetComponent<MeshFilter>());
                }
            }

            if (objectType == "billboard")
            {

                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.SetParent(tgt.scaledBody.GetComponentInChildren<SunCoronas>().transform);
                obj.GetComponent<Transform>().localPosition = Vector3.zero;
                obj.GetComponent<Transform>().localScale = dimensions;
                obj.GetComponent<Renderer>().material.shader = Shader.Find(shader);
                obj.GetComponent<Renderer>().material.SetTexture("_MainTex", GameDatabase.Instance.GetTexture(texturePath, false));
                obj.GetComponent<Transform>().rotation = rotation;
                obj.layer = tgt.scaledBody.layer;


                obj.name = name;

            }



        }

        //Code from the original INSTANTIATOR by Artyomka15
        public static void InvertNormals(MeshFilter filter)
        {
            Mesh mesh = filter.mesh;

            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;

            for (int m = 0; m < mesh.subMeshCount; m++)
            {
                int[] triangles = mesh.GetTriangles(m);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int temp = triangles[i + 0];
                    triangles[i + 0] = triangles[i + 1];
                    triangles[i + 1] = temp;
                }
                mesh.SetTriangles(triangles, m);
            }
        }


        /* FUTURE IDEAS
         * Add a particle system for maximum ring control?
         * Devise a method to load custom models?
        
    }
}
*/
