using System;
using System.Collections.Generic;
using UnityEngine;

namespace IKSpider.Orientation
{
    public class NormalFinder : MonoBehaviour
    {
        [SerializeField] private float _radius = 3f;
        [SerializeField] private float _cubeSize = .5f;
        [SerializeField] private bool _showNormals = false;
        [SerializeField] private bool _showGrid = false;

        private List<NodeCube> _nodes = null;

        private List<Vector3> _hitNormals = new List<Vector3>();

        // for debug puporses
        private List<Vector3> _hitPoints = new List<Vector3>();

        private Vector3 _totalNormal = Vector3.up;

        private void OnValidate()
        {
            _nodes = null;
            new NodeCube(null, TypeOfPrecedingCube.None, transform, Vector3.zero, _cubeSize, _radius, ref _nodes);
            Debug.Log("Number of nodes is " + _nodes.Count);
        }

        private void Update()
        {
            _hitNormals.Clear();
            _hitPoints.Clear();

            InterateThroughNodes(node => node.Reset());

            // check starts from the first node
            _nodes[0].StartCheckingFromTheNode();

            InterateThroughNodes(node =>
            {
                if (node.State == StateOfCube.Collided)
                {
                    node.GenerateNormals();
                }
            });

            InterateThroughNodes(node =>
            {
                if (node.State == StateOfCube.Collided)
                {
                    _hitNormals.AddRange(node.HitNormals);
                    _hitPoints.AddRange(node.HitPoints);
                }
            });

            if (_showNormals)
            {
                DrawDebugNormals();
            }
        }

        private void InterateThroughNodes(Action<NodeCube> action)
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                action?.Invoke(_nodes[i]);
            }
        }

        public Vector3 GetTotalNormal()
        {

            Vector3 resultNormal = Vector3.zero;

            foreach (var normal in _hitNormals)
            {
                resultNormal += normal;
            }

            resultNormal = resultNormal.normalized;
            return resultNormal;
        }

        public void DrawDebugNormals()
        {
            for (int i = 0; i < _hitNormals.Count; i++)
            {
                Debug.DrawLine(_hitPoints[i], _hitPoints[i] + _hitNormals[i], new Color(1, 0, 0, .2f));
            }

            Debug.DrawLine(transform.position, transform.position + _totalNormal);
        }

        private void OnDrawGizmos()
        {
            if (_showGrid)
            {
                Gizmos.color = Color.black * .1f;
                Gizmos.DrawWireSphere(transform.position, _radius);

                if (_nodes != null)
                {
                    foreach (var node in _nodes)
                    {
                        if (node.State == StateOfCube.Collided)
                        {
                            Gizmos.color = new Color(1, 0, 0, .2f);
                        }
                        else if (node.State == StateOfCube.NotCollided)
                        {
                            Gizmos.color = new Color(0, 0, 1, .2f);
                        }
                        else
                        {
                            Gizmos.color = new Color(1, 1, 1, .2f);
                        }

                        Gizmos.DrawWireCube(node.Position, node.Size * Vector3.one);
                    }
                }
            }
        }
    }
}