using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	public float moveSensitivityX = 1f;
	public float moveSensitivityY = 1f;
	public bool updateZoomSensitivity = true;
	public float orthoZoomSpeed = 0.5f;
	public float minZoom = 1f;
	public float maxZoom = 20f;
	public bool invertMoveX = false;
	public bool invertMoveY = false;
	public float mapWidth = 60.0f;
	public float mapHeight = 40.0f;

	private Camera _camera;
	private float minX, maxX, minY, maxY;
	private float horizontalExtent, verticalExtent;

	// Use this for initialization
	void Start () {
		
		_camera = Camera.main;

		maxZoom = 0.5f * (mapWidth / _camera.aspect);
		if (mapWidth > mapHeight) {
			maxZoom = 0.5f * mapHeight;
		}

		//if (_camera.orthographicSize > maxZoom)
		//	_camera.orthographicSize = maxZoom;

		CalculateLevelBounds ();

	}
	
	// Update is called once per frame
	void Update () {
		if (updateZoomSensitivity) {
			moveSensitivityX = _camera.orthographicSize / 5.0f;
			moveSensitivityY = _camera.orthographicSize / 5.0f;
		}

		Touch[] touches = Input.touches;

		if (touches.Length > 0) {
			//single touch (move)
			if (touches.Length == 1) {
				if (touches [0].phase == TouchPhase.Moved) {
					Vector2 delta = touches [0].deltaPosition;

					float positionX = delta.x * moveSensitivityX * Time.deltaTime;
					positionX = invertMoveX ? positionX : positionX * -1f;

					float positionY = delta.y * moveSensitivityY * Time.deltaTime;
					positionY = invertMoveY ? positionY : positionY * -1f;

					_camera.transform.position += new Vector3 (positionX, positionY, 0f);
				}
			}

			//double touch (zoom)
			if (touches.Length == 2) {
				Vector2 cameraViewSize = new Vector2 (_camera.pixelWidth, _camera.pixelHeight);


				Touch touchOne = touches [0];
				Touch touchTwo = touches [1];

				Vector2 touchOnePreviousPos = touchOne.position - touchOne.deltaPosition;
				Vector2 touchTwoPreviousPos = touchTwo.position - touchTwo.deltaPosition;

				float previousTouchDeltaMagnitude = (touchOnePreviousPos = touchTwoPreviousPos).magnitude;
				float touchDeltaMagnitude = (touchOne.position - touchTwo.position).magnitude;

				float deltaMagDifference = previousTouchDeltaMagnitude - touchDeltaMagnitude;

				_camera.transform.position += _camera.transform.TransformDirection ((touchOnePreviousPos + touchTwoPreviousPos - cameraViewSize) * _camera.orthographicSize / cameraViewSize.y);

				_camera.orthographicSize += deltaMagDifference * orthoZoomSpeed;
				_camera.orthographicSize = Mathf.Clamp (_camera.orthographicSize, minZoom, maxZoom) - 0.001f;

				_camera.transform.position -= _camera.transform.TransformDirection ((touchOne.position + touchTwo.position - cameraViewSize) * _camera.orthographicSize / cameraViewSize.y);

				CalculateLevelBounds ();

			}

		}
	}

	void CalculateLevelBounds ()
	{
		verticalExtent = _camera.orthographicSize;
		horizontalExtent = _camera.orthographicSize * Screen.width / Screen.height;
		minX = horizontalExtent - mapWidth / 2.0f;
		maxX = mapWidth / 2.0f - horizontalExtent;
		minY = verticalExtent - mapHeight / 2.0f;
		maxY = mapHeight / 2.0f - verticalExtent;
	}

	void LateUpdate ()
	{
		Vector3 limitedCameraPosition = _camera.transform.position;
		limitedCameraPosition.x = Mathf.Clamp (limitedCameraPosition.x, minX, maxX);
		limitedCameraPosition.y = Mathf.Clamp (limitedCameraPosition.y, minY, maxY);
		_camera.transform.position = limitedCameraPosition;
	}

	void OnDrawGizmos ()
	{
		Gizmos.DrawWireCube (Vector3.zero, new Vector3 (mapWidth, mapHeight, 0));
	}
	
}
