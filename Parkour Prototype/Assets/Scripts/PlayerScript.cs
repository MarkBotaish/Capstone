using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{

    public GameObject _cubePrefab;

    private Rigidbody _rigid;
    private GameObject _camera;
    private GameObject _placementCube;
    private GameObject _currentPlacementObject;

    private bool _isTouchingFloor = false;
    private bool _isAdjustingPlacement = false;
    private float _strafeMagnitude = 0f;
    private float _cubeHalfSize;
    private int _placementLayerMask;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _rigid = GetComponent<Rigidbody>();
        _camera = transform.Find("Main Camera").gameObject;
        _placementCube = transform.Find("TestPlacementCube").gameObject;
        _cubeHalfSize = _placementCube.GetComponent<MeshRenderer>().bounds.size.y / 2.0F;
        _placementLayerMask = LayerMask.GetMask("Floor");
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckPlacement();
    }

    void CheckPlacement()
    {
        if (!_isAdjustingPlacement)
        {
            bool hasHit = Physics.Raycast(_camera.transform.position, _camera.transform.forward, out RaycastHit hit, 50, _placementLayerMask);
            if (hasHit)
            {
                _placementCube.transform.position = hit.point + hit.normal * _cubeHalfSize;
                if (!_placementCube.activeSelf)
                    _placementCube.SetActive(true);

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    _placementCube.SetActive(false);
                    _isAdjustingPlacement = true;
                    _currentPlacementObject = Instantiate(_cubePrefab, _placementCube.transform.position, _placementCube.transform.rotation);
                }                         
                               
            }
            else
                _placementCube.SetActive(false);
        }
        else
        {
            Rigidbody rb = _currentPlacementObject.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            if (Input.GetKeyDown(KeyCode.Return))
            {
                _placementCube.transform.parent = transform;
                _isAdjustingPlacement = false;
            }

            if (Input.GetKey(KeyCode.DownArrow))
                rb.velocity += -_currentPlacementObject.transform.up;
            if (Input.GetKey(KeyCode.RightArrow))
                rb.velocity += _currentPlacementObject.transform.right;
            if (Input.GetKey(KeyCode.UpArrow))
                rb.velocity += _currentPlacementObject.transform.up;
            if (Input.GetKey(KeyCode.LeftArrow))
                rb.velocity += -_currentPlacementObject.transform.right;

            if(!Input.GetKey(KeyCode.LeftShift))
                rb.MoveRotation(Quaternion.Euler(_currentPlacementObject.transform.localEulerAngles + new Vector3(0, -Input.mouseScrollDelta.y * 5, 0)));   
            else
                rb.MoveRotation(Quaternion.Euler(_currentPlacementObject.transform.localEulerAngles + new Vector3(0,0, -Input.mouseScrollDelta.y * 5)));
        }  
    }

    void CheckInput()
    {
        _isTouchingFloor = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.05f, ~9);
        if (_isTouchingFloor && Input.GetKeyDown(KeyCode.Space))
        {
            _strafeMagnitude = new Vector3(_rigid.velocity.x, 0, _rigid.velocity.z).magnitude;
            _rigid.AddForce(Vector3.up * 10, ForceMode.VelocityChange);
        }
           
        Vector3 direction = GetMovementDirection() * Time.deltaTime * 1000.0f;
        _rigid.velocity = new Vector3(direction.x, _rigid.velocity.y, direction.z);
        _rigid.velocity += (Vector3.up * Physics.gravity.y * 1.5f * Time.deltaTime);

        Vector2 mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * -1);
        float xRot = Mathf.DeltaAngle(0, _camera.transform.localEulerAngles.x);
        xRot += mouse.y;

        if (Mathf.Abs(xRot) <= 53.0f)
            _camera.transform.Rotate(new Vector3(mouse.y, 0, 0));

        gameObject.transform.Rotate(new Vector3(0, mouse.x, 0));

        Debug.DrawRay(gameObject.transform.position, Vector3.down * 1.05f, Color.red);

        
    }

    Vector3 GetMovementDirection()
    {
        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            direction += transform.forward;
        
        if (Input.GetKey(KeyCode.S))
            direction -= transform.forward;
        
        if (Input.GetKey(KeyCode.A))
            direction -= transform.right;
        
        if (Input.GetKey(KeyCode.D))
            direction += transform.right;
       
        return direction;
    }
}
