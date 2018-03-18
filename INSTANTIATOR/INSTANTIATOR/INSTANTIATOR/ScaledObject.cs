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
}
