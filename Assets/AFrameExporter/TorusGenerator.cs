/**
 * Based on a script by thieberson (http://forum.unity3d.com/threads/torus-in-unity.8487/) (in Torus.zip, originally named Torus.cs),
 * which was based on a script by Steffen ("Primitives.cs" from $primitives_966_104.zip).
 * 
 * Editted by Michael Zoller on December 6, 2015.
 * 
 * Usage Notes:
 * If the color property is preventing you from changing the color in a different Script's start, give this script Execution Order priority.
 * 
 * Paraphrase of original usage notes:
 * This version is improved from Steffen's original to allow the manipulation of the ring outside the script (ex. in the Unity editor while testing).
 * The script can be attached to any GameObject (Main), although an Emtpy one is best.
 * When the script starts, it creates a sibling GameObject to be the ring (meshRing).
 * The user can change the segmentRadius, tubeRadius and numTubes of the meshRing through the
 * transform.scale.x, transform.scale.y and transform.scale.z, respectively, of Main.
 * The position, rotation and color of the meshRing are copied from Main.
 * 
 * Outside the script, the transform.scale of Main can be accessed by: GameObject.Find(name_of_the_Main_Game_Object)
 */
using UnityEngine;

public class TorusGenerator : MonoBehaviour {

	#region Manipulate Torus
    // If you don't need to see what values they were assigned, these can be made non-public
	public float segmentRadius = 1f;
	public float tubeRadius = 0.1f;
	public int numSegments = 32;
	public int numTubes = 12;

    public void torus(GameObject torusMesh, float segmentRadius, float tubeRadius, int numSegments, int numTubes)
    {
        // Total vertices
        int totalVertices = numSegments * numTubes;

        // Total primitives
        int totalPrimitives = totalVertices * 2;

        // Total indices
        int totalIndices = totalPrimitives * 3;

        // Init the mesh
        Mesh mesh = new Mesh();

        // Init the vertex and triangle arrays
        Vector3[] vertices = new Vector3[totalVertices];
        int[] triangleIndices = new int[totalIndices];

        // Calculate size of a segment and a tube
        float segmentSize = 2 * Mathf.PI / (float)numSegments;
        float tubeSize = 2 * Mathf.PI / (float)numTubes;

        // Create floats for our xyz coordinates
        float x, y, z;

        // Begin loop that fills in both arrays
        for (int i = 0; i < numSegments; i++)
        {
            // Find next (or first) segment offset
            int n = (i + 1) % numSegments; // changed segmentList.Count to numSegments

            // Find the current and next segments
            int currentTubeOffset = i * numTubes;
            int nextTubeOffset = n * numTubes;

            for (int j = 0; j < numTubes; j++)
            {
                // Find next (or first) vertex offset
                int m = (j + 1) % numTubes; // changed currentTube.Count to numTubes

                // Find the 4 vertices that make up a quad
                int iv1 = currentTubeOffset + j;
                int iv2 = currentTubeOffset + m;
                int iv3 = nextTubeOffset + m;
                int iv4 = nextTubeOffset + j;

                // Calculate X, Y, Z coordinates.
                x = (segmentRadius + tubeRadius * Mathf.Cos(j * tubeSize)) * Mathf.Cos(i * segmentSize);
                z = (segmentRadius + tubeRadius * Mathf.Cos(j * tubeSize)) * Mathf.Sin(i * segmentSize);
                y = tubeRadius * Mathf.Sin(j * tubeSize);

                // Add the vertex to the vertex array
                vertices[iv1] = new Vector3(x, y, z);

                // "Draw" the first triangle involving this vertex
                triangleIndices[iv1 * 6] = iv1;
                triangleIndices[iv1 * 6 + 1] = iv2;
                triangleIndices[iv1 * 6 + 2] = iv3;
                // Finish the quad
                triangleIndices[iv1 * 6 + 3] = iv3;
                triangleIndices[iv1 * 6 + 4] = iv4;
                triangleIndices[iv1 * 6 + 5] = iv1;
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = triangleIndices;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.Optimize();
        MeshFilter mFilter = torusMesh.GetComponent<MeshFilter>();
        mFilter.mesh = mesh;
    }
	#endregion

    public string torusGameObjectName = "TorusMesh";
    public Color color = Color.blue;
	GameObject torusMesh;
	Vector3 oldScale = Vector3.zero;
    Color oldColor = Color.clear;

	void Start () {
		torusMesh = new GameObject(torusGameObjectName);
        torusMesh.transform.parent = this.transform.parent;
		torusMesh.AddComponent<MeshFilter>();
		torusMesh.AddComponent<MeshRenderer>();
        Update(); // to allow other Script's Start() methods to change the color
	}
	
	void Update () {
		if (oldScale != transform.localScale) { // Chech if the parameters changed to improve performance
			segmentRadius = transform.localScale.x;
			tubeRadius = transform.localScale.y;
            numSegments = Mathf.RoundToInt(32 * (1 + segmentRadius / 10.0f));
			numTubes = Mathf.RoundToInt(transform.localScale.z);
			torus (torusMesh, segmentRadius, tubeRadius, numSegments, numTubes);
			oldScale = transform.localScale;
		}
        if (oldColor != color)
        {
            torusMesh.GetComponent<Renderer>().material.color = color;
            oldColor = color;
        }
		torusMesh.transform.position = transform.position;
		torusMesh.transform.rotation = transform.rotation;
	}
}
