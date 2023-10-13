shader_type canvas_item;

uniform vec4 line_color : hint_color = vec4(1.0);
uniform float line_thickness : hint_range(0, 10) = 1.0;

void fragment() {
    vec2 size = TEXTURE_PIXEL_SIZE * line_thickness;
    float outline = 0.0;

    outline += texture(TEXTURE, UV + size * vec2(-1, -1)).a;
    outline += texture(TEXTURE, UV + size * vec2(-1, 0)).a;
    outline += texture(TEXTURE, UV + size * vec2(-1, 1)).a;
    outline += texture(TEXTURE, UV + size * vec2(0, -1)).a;
    outline += texture(TEXTURE, UV + size * vec2(0, 1)).a;
    outline += texture(TEXTURE, UV + size * vec2(1, -1)).a;
    outline += texture(TEXTURE, UV + size * vec2(1, 0)).a;
    outline += texture(TEXTURE, UV + size * vec2(1, 1)).a;

    outline = min(outline, 1.0);

    vec4 color = texture(TEXTURE, UV);
    COLOR = mix(color, line_color, outline - color.a);
}