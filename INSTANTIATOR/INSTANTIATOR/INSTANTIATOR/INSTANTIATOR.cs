/*
 Created by Artyomka15 on the KSP forums
 1.3 port and additional editing done by The White Guardian on the KSP forums
 Commits and implemented suggestions will also be credited here
*/
﻿
//Grab Unity
using UnityEngine;
using System.Collections;

namespace INSTANTIATOR
{
    //Grab KSP
    [KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class ConfigLoader : MonoBehaviour //Monobehaviour script.
    {
        static Shader TWG_Ring_Shader;
        static ConfigNode ObjectSettings;
        //Start loads on startup, perfect for a one-time object insertion.
        void Start()
        {
            //Grab all external data
            TWG_Ring_Shader = GameDatabase.Instance.GetShader(KSPUtil.ApplicationRootPath + "GameData/INSTANTIATOR/Ring.shader");

            //Grab the config file, for future use: devise way to scan for INSTANTIATOR config files?
            ObjectSettings = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/INSTANTIATOR/Objects.cfg");

            //Now read the config
            ReadConfiguration();
            
        }
        

        
        
        //Move to new void, possible future in-game editing by re-calling the void?
        void ReadConfiguration()
        {
            

            foreach(ConfigNode entry in ObjectSettings.GetNodes("INSTANTIATOR"))
            {
                //Search through the config for a SCALED_OBJECT node
                foreach (ConfigNode node in entry.GetNodes("SCALED_OBJECT"))
                {
                    //Give the GameObject a unique name for Unity's sake
                    string name = node.GetValue("name");
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
                    //Call the function that places the object
                    PlaceObject(name, type, dimensions, shader, tex, rot, bodyName, InvertNormals, scale, offset);
                }

                //Search through the config for a TWG node
                foreach (ConfigNode node in entry.GetNodes("TWG_RING"))
                {
                    //Object name
                    string name = node.GetValue("name");
                    //Object scale
                    Vector3 scale = ConfigNode.ParseVector3(node.GetValue("scale"));
                    //Parent body
                    string parentBody = node.GetValue("parent");
                    //Rotation of the object
                    Quaternion rot = Quaternion.Euler(ConfigNode.ParseVector3(node.GetValue("rotation")));
                    //Main texture
                    string mainTex = node.GetValue("mainTex");
                    //Main normals (normals provide light artifacts to create 'fake geometry' - 3D effects but the computer doesn't have to render the whole mesh, just light impact calculations
                    string mainNrm = node.GetValue("mainNrm");
                    //Main color (the texture is multiplied by this color, so use it to recolor)
                    Vector4 colVec = ConfigNode.ParseVector4(node.GetValue("color"));
                    //Convert vector4 to color while accounting for the 255-style format used by image editing software
                    Color col = new Color(colVec.x / 255, colVec.y / 255, colVec.z / 255, colVec.w / 255);
                    //Shininess. Scaled 0-1, this value determines the intenisty of light reflectance by the ring. Greater values will even result in a mirror-like effect.
                    float shine = float.Parse(node.GetValue("shininess"));
                    //Specular. Basically the intensity of specular lighting on a scale of 0-1.
                    float spec = float.Parse(node.GetValue("specular"));
                    //Emission. This texture makes the rings emit light on greater intensities (removes shadow cast on these parts)
                    string mainEmi = node.GetValue("emission");
                    //Transmission. This texture determines how much ambient light each part of the ring gets.
                    string mainTrans = node.GetValue("transmission");
                    //Detail texture. This is layed on top of the main texture to provide detail even at close range.
                    string detail = node.GetValue("detail");
                    //Detail scale and offset. Basically tiles and offsets the texture for maximum control.
                    Vector4 detailSettings = ConfigNode.ParseVector4(node.GetValue("detailSettings"));
                    //Format: 'detailSettings = scale x, scale y, offset x, offset y'

                    //Place the object
                    Finalize(name, scale, parentBody, rot, mainTex, mainNrm, col, shine, spec, mainEmi, mainTrans, detail, detailSettings);
                }
            }
        }

        //Actually place the planets
        public static void Finalize(string n, Vector3 s, string parent, Quaternion rotation, string mTex, string mNrm, Color mC, float Shiny, float Specular, string emission, string transmission, string detail, Vector4 settings)
        {
            //Target a celestial
            CelestialBody Target = FlightGlobals.GetBodyByName(parent);
            //Create a new GameObject. Instead of a cube, this method creates a plane, resulting in only 2 faces. The custom shader that comes with this unofficial INSTANTIATOR has backface culling off, so it should be visible on both sides.
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Plane);

            //Parent it
            obj.transform.SetParent(Target.scaledBody.transform);

            //Recalculate
            obj.GetComponent<MeshFilter>().mesh.RecalculateBounds();
            obj.GetComponent<MeshFilter>().mesh.RecalculateNormals();

            //Reposition
            obj.GetComponent<Transform>().localPosition = Vector3.zero;

            
            //Scale it up!
            obj.GetComponent<Transform>().localScale = s;

            //Shader
            obj.GetComponent<Renderer>().material.shader = TWG_Ring_Shader;

            //Set all of the shader values
            obj.GetComponent<Renderer>().material.SetTexture("_MainTex", GameDatabase.Instance.GetTexture(mTex, false));
            obj.GetComponent<Renderer>().material.SetTexture("_BumpMap", GameDatabase.Instance.GetTexture(mNrm, true));
            obj.GetComponent<Renderer>().material.SetFloat("_Specular", Specular);
            obj.GetComponent<Renderer>().material.SetFloat("_Shininess", Shiny);
            obj.GetComponent<Renderer>().material.SetTexture("_Emission", GameDatabase.Instance.GetTexture(emission, false));
            obj.GetComponent<Renderer>().material.SetTexture("_Transmission", GameDatabase.Instance.GetTexture(transmission, false));
            obj.GetComponent<Renderer>().material.SetTexture("_Detail", GameDatabase.Instance.GetTexture(detail, false));
            obj.GetComponent<Renderer>().material.SetTextureScale("_Detail", new Vector2(settings.x, settings.y));
            obj.GetComponent<Renderer>().material.SetTextureOffset("_Detail", new Vector2(settings.z, settings.w));

        }

        //For the 'SCALED_OBJECT' nodes
        public static void PlaceObject(string name, string objectType, Vector3 dimensions, string shader, string texturePath, Quaternion rotation, string getBodyByName, bool invert, Vector2 scale, Vector2 offset)
        {
            //Find the target celestial
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
                obj.layer = tgt.scaledBody.layer;



                obj.name = name;

                if (invert == true)
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
        */
    }
}
