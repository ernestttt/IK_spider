using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class IK_leg : MonoBehaviour
{
    [SerializeField] private Transform _base;
    [SerializeField] private Transform _target;

    [SerializeField] private MeshRenderer[] _chain_renderers;

    private ChainParam[] _chains;

    private void Awake()
    {
        _chains = new ChainParam[_chain_renderers.Length];
        for (int i = 0; i < _chain_renderers.Length; i++){
            _chains[i] = new ChainParam(_chain_renderers[i]);
        }
    }

    private void Start(){

    }

    private void Update(){
        Vector3 fromBase2Target = _target.position - _base.position;
        Debug.DrawLine(_base.position, _base.position + fromBase2Target, Color.white);

        // set rotations
        for (int i = 0; i < _chains.Length; i++){
            _chains[i].Rotation = Quaternion.FromToRotation(_chains[i].Direction, fromBase2Target) * _chains[i].Rotation;
        }

        // set positions
        for(int i = 0; i < _chains.Length; i++){
            Vector3 diff = _base.position - _chains[i].BasePos;
           _chains[i].ChangeChainPositionBy(diff);
        }
    }

    private void OnDrawGizmos()
    {
        float sphereRadius = 0.003f;
        for(int i = 0; _chains != null && i < _chains.Length; i++){
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_chains[i].BasePos, sphereRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_chains[i].EndPos, sphereRadius);
        }
        
    }
}

public struct ChainParam{

    private float _lenght;
    private Transform _transform;

    public float Lenght => _lenght;
    public Vector3 BasePos => _transform.position - _transform.right * _lenght * 0.5f;
    public Vector3 EndPos => _transform.position + _transform.right * _lenght * 0.5f;

    public Vector3 Direction => _transform.right;

    public Quaternion Rotation {
        get{
            return _transform.rotation;
        }
        set{
            _transform.rotation = value;
        }
    }

    public ChainParam(MeshRenderer renderer){
        _lenght = renderer.transform.lossyScale.x;
        _transform = renderer.transform;
    }

    public void ChangeChainPositionBy(Vector3 delta){
        _transform.position += delta;
    }
}
