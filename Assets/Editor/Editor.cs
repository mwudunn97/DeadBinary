using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

// Used for custom inspector widgets


[CustomEditor(typeof(Tile))]
public class TileCustom : Editor
{
	private Tile script;
    Dictionary<string, CoverObject> coverDict;
    string[] _choices = new[] { "Half Wall", "Full Wall" };
    int _choiceIndex = 0;

    private void OnEnable()
	{
        // Method 1
        script = (Tile) target;
    }

    [ExecuteInEditMode]
    public override void OnInspectorGUI()
    {
        AssetManager assetManager = GameObject.FindGameObjectWithTag("GlobalManager").GetComponent<AssetManager>();
        coverDict = new Dictionary<string, CoverObject> { { "Half Wall", assetManager.cover.halfWall }, { "Full Wall", assetManager.cover.fullWall } };

        EditorGUILayout.BeginHorizontal();

        // Drop down choices for cover prefab
        _choiceIndex = EditorGUILayout.Popup(_choiceIndex, _choices, GUILayout.Width(120));
        script.choice = _choices[_choiceIndex];

        GUILayout.FlexibleSpace();
        // Creates a new cover object is none exists for tile already
        if (GUILayout.Button(" > Create Cover", EditorStyles.miniButtonLeft, GUILayout.Width(120)))
        {
            if (!script.cover)
            {
                
                CoverObject newCover = Instantiate(coverDict[script.choice], script.transform.position, new Quaternion(0,0,0,0), assetManager.mapObjects);
                newCover.transform.position = script.transform.position + new Vector3(0f, 0f, 1f);
                script.cover = newCover;
                //script.occupant.transform.localScale = new Vector3(2f, 0.85f, 0.5f);
                //script.SetNeighboursCover();
                EditorUtility.SetDirty(script);
            }
        }

        // Rotates existing cover object for tile
        if (GUILayout.Button(" > Rotate Cover", EditorStyles.miniButtonMid, GUILayout.Width(120)))
        {
            if (script.cover is CoverObject)
            {
                Transform parent = script.GetComponentInParent<Tile>().transform;
                script.cover.transform.RotateAround(parent.position, Vector3.up, 90);
                EditorUtility.SetDirty(script);
            }
        }

        // Deletes existing cover object for tile
        if (GUILayout.Button(" > Remove Cover", EditorStyles.miniButtonRight, GUILayout.Width(120)))
        {
            if (script.cover is CoverObject)
            {
                DestroyImmediate(script.cover.gameObject);
                script.cover = null;
                EditorUtility.SetDirty(script);
            }
        }
        EditorGUILayout.EndHorizontal();

        //	Draw default inspector after button...
        base.OnInspectorGUI();
    }
}