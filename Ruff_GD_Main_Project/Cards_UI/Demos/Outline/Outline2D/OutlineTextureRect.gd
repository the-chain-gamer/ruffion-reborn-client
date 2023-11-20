extends TextureRect

export var line_color := Color.white
export var isInGameplay := false;

onready var _tween: Tween = $Tween



func _ready() -> void:
	self.connect("mouse_entered", self, "_on_Area2D_mouse_entered")
	self.connect("mouse_exited", self, "_on_Area2D_mouse_exited")
	line_color.a = 0
	print("hello")


func _on_Area2D_mouse_entered() -> void:
	_tween.interpolate_method(
		self, "outline_alpha", line_color.a, 1.0, 0.25, Tween.TRANS_LINEAR, Tween.EASE_OUT
	)
	_tween.start()
	if(isInGameplay == true):
		rect_scale = Vector2(2.5, 2.5);
		var parentNode = get_parent();
		parentNode.call("OnMouseEnter");
	


func _on_Area2D_mouse_exited() -> void:
	_tween.interpolate_method(
		self, "outline_alpha", line_color.a, 0.0, 0.25, Tween.TRANS_LINEAR, Tween.EASE_IN
	)
	_tween.start()
	if(isInGameplay == true):
		rect_scale = Vector2(1, 1);
		var parentNode = get_parent();
		parentNode.call("OnMouseExit");


func outline_alpha(value: float) -> void:
	line_color.a = value
	material.set_shader_param("line_color", line_color)
