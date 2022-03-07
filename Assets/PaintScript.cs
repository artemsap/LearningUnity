using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Brush
{
    Circle,
    Quad    
}

public class PaintScript : MonoBehaviour
{
    [Range(2, 512)]
    [SerializeField] private int _textureSize = 128;
    [SerializeField] private TextureWrapMode _textureWrapMode = TextureWrapMode.Clamp;
    [SerializeField] private FilterMode _filter = FilterMode.Point;
    [SerializeField] private Material _material;

    [SerializeField] private Camera _camera;
    [SerializeField] private Color _brushColor;
    [SerializeField] private int _brushSize = 5;
    [SerializeField] private Brush _brush = Brush.Quad;
    [SerializeField] private bool _mixing = false;
    [Range(0, 1)]
    [SerializeField] private float _brushHardness = 1;

    private Collider _collider;
    private Texture2D _texture;
    private int oldX; 
    private int OldY;

    private void OnValidate()
    {
        if (_texture == null)
            _texture = new Texture2D(_textureSize, _textureSize);

        if (_collider == null)
            _collider = GetComponent<Collider>();

        if (_texture.width != _textureSize)
            _texture.Resize(_textureSize, _textureSize);

        _texture.wrapMode = _textureWrapMode;
        _texture.filterMode = _filter;
        _material.mainTexture = _texture;
        _texture.Apply();
    }

    // Update is called once per frame
    void Update()
    {
        _brushSize += (int)Input.mouseScrollDelta.y;

        if (Input.GetMouseButton(0))
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (_collider.Raycast(ray, out hit, 100f))
            {
                int RayX = (int)(hit.textureCoord.x * _textureSize);
                int RayY = (int)(hit.textureCoord.y * _textureSize);

                if (RayX != oldX || RayY != OldY)
                {
                    switch (_brush)
                    {
                        case Brush.Quad:
                            PaintQuad(RayX, RayY);
                            break;
                        case Brush.Circle:
                            PaintCircle(RayX, RayY);
                            break;
                        default:
                            PaintQuad(RayX, RayY);
                            break;
                    }
                    _texture.Apply();
                }

                oldX = RayX;
                OldY = RayY;

            }
        }
    }

    void PaintQuad(int RayX, int RayY)
    {
        for (int x = -_brushSize / 2; x < _brushSize / 2; x++)
            for (int y = -_brushSize / 2; y < _brushSize / 2; y++)
            {
                var Xpixel = RayX + x;
                var Ypixel = RayY + y;

                float rIn = _brushSize * _brushHardness / 2;
                float rOut = _brushSize / 2;
                float interpolate = Mathf.InverseLerp(rIn * rIn, rOut * rOut, x * x);
                float interpolate1 = Mathf.InverseLerp(rIn * rIn, rOut * rOut, y * y);

                float finalInterpolate;

                if (interpolate > interpolate1)
                    finalInterpolate = interpolate;
                else
                    finalInterpolate = interpolate1;

                var tmpColor = Color.Lerp(_brushColor, new Color(_brushColor.r, _brushColor.g, _brushColor.b, 0), finalInterpolate);

                if (_mixing)
                {
                    var Oldcolor = _texture.GetPixel(Xpixel, Ypixel);
                    var resultColor = Color.Lerp(Oldcolor, tmpColor, tmpColor.a);
                    _texture.SetPixel(Xpixel, Ypixel, resultColor);
                }
                else
                    _texture.SetPixel(Xpixel, Ypixel, tmpColor);
            }
               
    }

    void PaintCircle(int RayX, int RayY)
    {
        var CircleRad = _brushSize / 2;

        for (int x = -CircleRad; x < CircleRad; x++)
            for (int y = -CircleRad; y < CircleRad; y++)
            {
                float _x2 = x * x;
                float _y2 = y * y;
                float _r2 = (CircleRad - 0.5f) * (CircleRad - 0.5f);

                var Xpixel = RayX + x;
                var Ypixel = RayY + y;

                if (_x2 + _y2 < _r2)
                {
                    float rIn = CircleRad * _brushHardness * CircleRad * _brushHardness;
                    float interpolate = Mathf.InverseLerp(rIn, CircleRad*CircleRad, _x2 + _y2);
                    var tmpColor = Color.Lerp(_brushColor, new Color(_brushColor.r, _brushColor.g, _brushColor.b, 0), interpolate);

                    if (_mixing)
                    {
                        var Oldcolor = _texture.GetPixel(Xpixel, Ypixel);
                        var resultColor = Color.Lerp(Oldcolor, tmpColor, tmpColor.a);
                        _texture.SetPixel(Xpixel, Ypixel, resultColor);
                    }
                    else
                        _texture.SetPixel(Xpixel, Ypixel, tmpColor);
                }

                    
            }
    }

}
