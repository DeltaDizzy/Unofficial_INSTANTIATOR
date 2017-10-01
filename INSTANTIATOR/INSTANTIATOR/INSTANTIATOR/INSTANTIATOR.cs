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
        static ConfigNode ObjectSettings;
        //Start loads on startup, perfect for a one-time object insertion.
        void Start()
        {
            //Grab all external data
            
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
            }
        }
        
        //For the 'SCALED_OBJECT' nodes
        public static void PlaceObject(string name, string objectType, Vector3 dimensions, string shader, string texturePath, Quaternion rotation, string getBodyByName, bool invert, Vector2 scale, Vector2 offset)
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
    }
}
