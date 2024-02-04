using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class NormalFinder : MonoBehaviour
{
    [SerializeField] private float _radius = 3f;
    [SerializeField] private float _cubeSize = .5f;

    private NodeCube _raycastWeb;

    private List<NodeCube> _nodes = null;

    private List<Vector3> _hitNormals = new List<Vector3>();
    private List<Vector3> _hitPoints = new List<Vector3>();

    private void OnValidate(){
        _nodes = null;
        _raycastWeb = new NodeCube(null, TypeOfPrecedingCube.None, transform, Vector3.zero, _cubeSize, _radius, ref _nodes);
        Debug.Log("Number of nodes is " + _nodes.Count);
    }

    private void Update(){
        _hitNormals.Clear();
        _hitPoints.Clear();

        foreach (var node in _nodes){
            node.Reset();
        }

        _nodes[0].StartCheckingFromTheNode();

        foreach(var node in _nodes.Where(a => a.State == StateOfCube.Collided))
        {
            node.GenerateNormals();
        }

        foreach (var node in _nodes.Where(a => a.State == StateOfCube.Collided))
        {
            _hitNormals.AddRange(node.HitNormals);
            _hitPoints.AddRange(node.HitPoints);
        }

        DrawDebugNormals();
    }

    public void DrawDebugNormals(){
        for(int i = 0; i < _hitNormals.Count; i++){
            Debug.DrawLine(_hitPoints[i], _hitPoints[i] + _hitNormals[i], Color.red);
        }
    }

    private void OnDrawGizmos(){
        Gizmos.color = Color.black * .1f;   
        Gizmos.DrawWireSphere(transform.position, _radius);

        if(_nodes != null){
            foreach(var node in _nodes)
            {
                if(node.State == StateOfCube.Collided){
                    Gizmos.color = new Color(1, 0, 0, .1f);
                }
                else if (node.State == StateOfCube.NotCollided)
                {
                    Gizmos.color = new Color(0,0,1,.1f);
                }
                else{
                    Gizmos.color = new Color(1, 1, 1, .1f);
                }

                Gizmos.DrawWireCube(node.Position, node.Size * Vector3.one);
            }
        }
    }
}
