using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParkourScript : MonoBehaviour
{
    private Rigidbody _rigid;
    private GameObject _camera;

    private bool _isTouchingFloor = false;
    private bool _isTouchingWall = false;
    private float _strafeMagnitude = 0f;
    private float _wallRunningMultiplier = 1.5f;
    private Vector3 _wallVelocity = Vector3.zero;
    private Vector3 _wallNormal = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _rigid = GetComponent<Rigidbody>();
        _camera = transform.Find("Main Camera").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        CheckMouse();
        if (!_isTouchingWall)
            CheckInput();
        else
            WallRun();
    }

    void WallRun()
    {
        _rigid.velocity = _wallVelocity;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.DrawRay(gameObject.transform.position, (_wallNormal + Vector3.up * 0.5F + _wallVelocity.normalized * 2).normalized * 500.0f, Color.red);
            //Debug.Break();
            _rigid.AddForce((_wallNormal*0.25f + Vector3.up) * 10, ForceMode.VelocityChange);
            _isTouchingWall = false;
            print("Space");
        }
    }

    void CheckMouse()
    {
        Vector2 mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * -1);
        float xRot = Mathf.DeltaAngle(0, _camera.transform.localEulerAngles.x);
        xRot += mouse.y;

        if (Mathf.Abs(xRot) <= 53.0f)
            _camera.transform.Rotate(new Vector3(mouse.y, 0, 0));

        gameObject.transform.Rotate(new Vector3(0, mouse.x, 0));

        Debug.DrawRay(gameObject.transform.position, Vector3.down * 1.05f, Color.red);
    }

    void CheckInput()
    {
        _isTouchingFloor = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.05f, ~9);
        if (_isTouchingFloor && Input.GetKeyDown(KeyCode.Space))
        {
            _strafeMagnitude = new Vector2(_rigid.velocity.x, _rigid.velocity.z).magnitude;
            _rigid.AddForce(Vector3.up * 10, ForceMode.VelocityChange);
        }

        Vector3 direction = GetMovementDirection() * Time.deltaTime * 1000.0f;
        if (_isTouchingFloor)
            _rigid.velocity += new Vector3(direction.x, 0, direction.z);
        else
            _rigid.velocity += new Vector3(direction.x, 0, direction.z) * 0.03f;
                
        Vector2 horizontalMove = new Vector2(_rigid.velocity.x, _rigid.velocity.z);
        if(horizontalMove.magnitude > 7)
        {
            horizontalMove = horizontalMove.normalized * 7;
            _rigid.velocity = new Vector3(horizontalMove.x, _rigid.velocity.y, horizontalMove.y);
        }

        _rigid.velocity += (Vector3.up * Physics.gravity.y * 1.5f * Time.deltaTime);

        
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

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Wall")
        {
            _rigid.useGravity = false;
            _isTouchingWall = true;
            //_wallVelocity = new Vector2(_rigid.velocity.x, _rigid.velocity.y) * _wallRunningMultiplier;
            _wallNormal = collision.contacts[0].normal;

            Vector3 cross = Vector3.Cross(_wallNormal, Vector3.up);

            _wallVelocity = Vector3.Project(_rigid.velocity, cross) * _wallRunningMultiplier;
            Debug.DrawRay(collision.contacts[0].point, cross, Color.red);
            Debug.DrawRay(collision.contacts[0].point, _rigid.velocity, Color.blue);
            Debug.DrawRay(collision.contacts[0].point, Vector3.Project(_rigid.velocity, cross), Color.green);

        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            _rigid.useGravity = true;
            _isTouchingWall = false;
            _rigid.AddForce(_wallVelocity, ForceMode.VelocityChange);
            _wallVelocity = Vector3.zero;
        }

    }
}
