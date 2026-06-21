using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.Layouts;
public class Joystick : OnScreenControl 
{
    
	public RectTransform center;
	public RectTransform knob;
	public float range;
	public bool fixedJoystick;
	
	[HideInInspector]
	public Vector2 direction;
	
	Vector2 start;

	[InputControl(layout = "Vector2")]
	[SerializeField]
	private string m_ControlPath;

	protected override string controlPathInternal
	{
		get => m_ControlPath;
		set => m_ControlPath = value;
	}
	
	void Start(){
		ShowHide(false);
	}
	
	void Update(){
		// 严谨判断：如果没有任何指针输入（没鼠标也没触屏），直接返回
		if (Pointer.current == null) return;
		Vector2 pos = Pointer.current.position.ReadValue();
		
		if(Pointer.current.press.wasPressedThisFrame){
			ShowHide(true);
			start = pos;
			
			knob.position = pos;
			center.position = pos;
		}
		else if(Pointer.current.press.isPressed){
			knob.position = pos;
			knob.position = center.position + Vector3.ClampMagnitude(knob.position - center.position, center.sizeDelta.x * range);
			
			if(knob.position != (Vector3)pos && !fixedJoystick){
				Vector3 outsideBoundsVector = (Vector3)pos - knob.position;
				center.position += outsideBoundsVector;
			}
			
			direction = (knob.position - center.position).normalized;

			// 【关键点】：将摇杆的方向向量，实时发射给新输入系统
			SendValueToControl(direction);
		}
		else if(Pointer.current.press.wasReleasedThisFrame){
			ShowHide(false);
			direction = Vector2.zero;

			// 【关键点】：松开时，向新输入系统发送 0，让角色停下
			SendValueToControl(Vector2.zero);
		}
	}
	
	void ShowHide(bool state){
		center.gameObject.SetActive(state);
		knob.gameObject.SetActive(state);
	}
}
