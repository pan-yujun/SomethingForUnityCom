using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace IndieStudio.DrawingAndColoring.Logic
{
    public class MRDrawingScratch : MonoBehaviour, IMixedRealityFocusHandler, IMixedRealityInputHandler
    {
        private bool mIsPinching = false;
        private bool mIsEnterBoard = false;

        public DrawingMode mMode = DrawingMode.Pen;
        //笔的颜色
        public Color32 penColour = Color.black;
        //笔的宽度
        public int penWidth = 10;
        //画的图
        private Sprite drawSprite;
        //画的纹理
        private Texture2D drawableTexture2D;
        //之前画的位置
        private Vector2 previousDragPosition = Vector2.zero;
        //存放最初的颜色数组
        private Color32[] orignalColorArray;
        //存放目前的颜色数组
        private Color32[] currentColorArray;
        //存放画的前一张图，用于撤回
        private Color32[] previousColorArray;
        //图像(绘画区域)的宽高
        private int drawRectHeight;
        private int drawRectWidth;
        //当前是否在滑动
        private bool isDraging = false;
        //保存画过的步骤
        private Stack<Color32[]> savePixelStack;
        //撤销画过的步骤
        private Stack<Color32[]> revertPixelStack;

        private IMixedRealityPointer mCurPointer;

        [Header("以下属性为擦除功能使用")]
        public Texture2D maskTexture2D;
        public Texture2D brushTexture;
        private Image mImage;
        private Material scratchMaterial;
        private float mScratchProcess = 0;//擦除进度

        private void Awake()
        {
            savePixelStack = new Stack<Color32[]>();
            revertPixelStack = new Stack<Color32[]>();
            drawSprite = GetComponent<Image>().sprite;
            if (mMode == DrawingMode.Pen || mMode == DrawingMode.Eraser)
            {
                drawableTexture2D = drawSprite.texture;
            }
            else if(mMode == DrawingMode.Smear) // 擦除模式
            {
                if (mImage == null)
                    mImage = GetComponent<Image>();
                if (scratchMaterial == null)
                    scratchMaterial = mImage.material;
                penColour = Color.white;
                drawableTexture2D = maskTexture2D;
                scratchMaterial.SetTexture("_MaskTex", maskTexture2D);
            }
            orignalColorArray = drawableTexture2D.GetPixels32();
            //当前sprite各个像素的颜色
            currentColorArray = new Color32[orignalColorArray.Length];
            Array.Copy(orignalColorArray, currentColorArray, orignalColorArray.Length);

            drawRectHeight = drawableTexture2D.height;
            drawRectWidth = drawableTexture2D.width;
        }

        // Update is called once per frame
        void Update()
        {
            // 测试
            //UpdateTouch();
#if UNITY_WSA || UNITY_ANDROID
            Vector3? point = GetBoardPoint();
            if (point != null)
            { 
                UpdateDraging(point);
            }
#else
                        UpdateTouch();
#endif

            //if (Input.GetKeyDown(KeyCode.F1))
            //{
            //    Debug.Log("[MRDrawingScratch]F1");
            //    SaveImage();
            //}
            //else if (Input.GetKeyDown(KeyCode.F2))
            //{
            //    //CreateSoftBrushCor(32);
            //    //CreateAndSaveSoftBrushTexture(32, Application.dataPath + "/SavedBrush.png");
            //}
        }
        private Vector3? GetBoardPoint()
        {
            Vector3? hitPoint = null;
#if UNITY_WSA || UNITY_ANDROID
            if (mCurPointer != null)
            {
                hitPoint = mCurPointer.Result.Details.Point;
            }   
#endif
            return hitPoint;
        }
        private void UpdateDraging(Vector3? point)
        {
            // 绘画
            DrawInTexture(point);
        }
        private void UpdateTouch()
        {
            Vector3? hitPoint = null;
            Vector2? touchPosition = null;
            if (Input.touchSupported)
            {
                // 触摸屏触摸
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0); // 获取第一个触摸点
                    touchPosition = touch.position; // 获取触摸位置// 对2D游戏
                }
            }
            else
            {
                // 鼠标点击
                if (Input.GetMouseButton(0))
                {
                    touchPosition = Input.mousePosition;
                }
            }
            //Debug.Log($"[MRDrawingScratch]touchPosition:{touchPosition}");
            if (touchPosition != null)
            {
                Ray ray = Camera.main.ScreenPointToRay((Vector3)touchPosition);
                RaycastHit hit;

                // 检测射线是否碰到物体
                if (Physics.Raycast(ray, out hit))
                {
                    // 获取射线与物体的碰撞点
                    hitPoint = hit.point;
                    Debug.Log($"射线碰到的物体: {hit.collider.gameObject.name}, 碰撞点: {hitPoint}");

                    if (hit.collider.gameObject == this.gameObject)
                    {
                        if (!isDraging)
                        {
                            StartDraw();
                        }
                    }
                    else
                    {
                        if (isDraging)
                        {
                            StopDraw();
                        }
                    }
                }
                else
                {
                    if (isDraging)
                    {
                        StopDraw();
                    }
                }
            }
            else
            {
                if (isDraging)
                {
                    StopDraw();
                }
            }
            //if (Input.touchCount > 0)
            //{
            //    Touch touch = Input.GetTouch(0); // 获取第一个触摸点
            //    Vector2 touchPosition = touch.position; // 获取触摸位置// 对2D游戏
            //    Vector3 touchPosition3D = Camera.main.ScreenToWorldPoint(touch.position); // 对3D游戏
            //                                                                              // 创建一条从触摸点射出的射线
            //    Ray ray = Camera.main.ScreenPointToRay(touchPosition);
            //    RaycastHit hit;

            //    // 检测射线是否碰到物体
            //    if (Physics.Raycast(ray, out hit))
            //    {
            //        // 获取射线与物体的碰撞点
            //        hitPoint = hit.point;
            //        Debug.Log($"射线碰到的物体: {hit.collider.gameObject.name}, 碰撞点: {hitPoint}");

            //        if (hit.collider.gameObject == this.gameObject)
            //        {
            //            if (!isDraging)
            //            {
            //                StartDraw();
            //            }
            //        }
            //        else
            //        {
            //            if (isDraging)
            //            {
            //                StopDraw();
            //            }
            //        }
            //    }
            //    else
            //    {
            //        if (isDraging)
            //        {
            //            StopDraw();
            //        }
            //    }
            //}
            //else
            //{
            //    if (isDraging)
            //    {
            //        StopDraw();
            //    }
            //}
            if (hitPoint != null)
            {
                UpdateDraging(hitPoint.Value);
            }
        }

        void OnDestroy()
        {
            ResetBoard();

            if (scratchMaterial != null)
                Destroy(scratchMaterial);
        }

        #region 画图

        /// <summary>
        /// 在贴图上绘画
        /// </summary>
        /// <param name="point"></param>
        private void DrawInTexture(Vector3? point)
        {
            if (!isDraging)
            {
                return;
            }
            Vector3 hitPoint = (Vector3)point;
            //碰撞位置转化为对应sprite像素位置        
            Vector2 pixelPos = WorldToPixelCoordinates(hitPoint);
            //Debug.Log($"射线碰撞位置：{hitPoint}，{pixelPos}");
            //如果是0,0点就说明是重新开始画，就不用从之前的点lerp了
            if (previousDragPosition == Vector2.zero)
            {
                previousDragPosition = pixelPos;
            }
            //计算当前的位置和上一次记录的位置之间的距离，然后平滑的画，这是为了防止移动太快，画的点不连续
            float distance = Vector2.Distance(previousDragPosition, pixelPos);
            //float steps = 1 / distance;
            float steps = penWidth / distance;
            for (float lerp = 0; lerp <= 1; lerp += steps)
            {
                //插值
                Vector2 curPosition = Vector2.Lerp(previousDragPosition, pixelPos, lerp);
                //画
                PenDraw(curPosition);
            }
            previousDragPosition = pixelPos;

            drawableTexture2D.SetPixels32(currentColorArray);
            drawableTexture2D.Apply();
        }
        /// <summary>
        /// 在图形上画
        /// </summary>
        /// <param name="pixelPos"></param>
        private void PenDraw(Vector2 pixelPos)
        {
            MarkPixelsToColour(pixelPos, penWidth);
        }
        /// <summary>
        /// 在颜色数组中找到点击的像素的位置，并更改颜色
        /// </summary>
        /// <param name="centerPixel"></param>
        /// <param name="penWidth"></param>
        private void MarkPixelsToColour(Vector2 centerPixel, int penWidth)
        {
            // 中心位置X
            int centerX = (int)centerPixel.x;
            // 中心位置Y
            int centerY = (int)centerPixel.y;
            // 笔刷的半径
            int brushRadius = brushTexture.width / 2;
            for (int x = -penWidth; x <= penWidth; x++)
            {
                for (int y = -penWidth; y <= penWidth; y++)
                {
                    int pixelX = centerX + x;
                    int pixelY = centerY + y;
                    // 边界外不画
                    if (pixelX >= drawRectWidth || pixelX < 0 || pixelY >= drawRectHeight || pixelY < 0)
                        continue;

                    // 计算在brushTexture上的采样坐标
                    float sampleX = (x + penWidth) / (2.0f * penWidth) * brushTexture.width;
                    float sampleY = (y + penWidth) / (2.0f * penWidth) * brushTexture.height;

                    // 获取笔刷纹理的颜色
                    Color brushColor = brushTexture.GetPixel((int)sampleX, (int)sampleY);

                    // 计算目标像素位置
                    int arrayPos = pixelY * drawRectWidth + pixelX;

                    // 混合颜色
                    currentColorArray[arrayPos] = Color32.Lerp(currentColorArray[arrayPos], penColour, brushColor.a);
                }
            }
        }

        /// <summary>
        /// 将鼠标的世界坐标转化为图片的本地坐标左下角(-256,-256)右上角(256,256)
        /// </summary>
        /// <param name="mouseWorldPosition"></param>
        /// <returns></returns>
        private Vector2 WorldToPixelCoordinates(Vector3 mouseWorldPosition)
        {
            //将位置从世界空间转换为局部空间。
            //transformA.InverseTransformPoint(transformB.position),获取transfromB相对于transformA的局部坐标
            Vector2 localPos = transform.InverseTransformPoint(mouseWorldPosition);
            //localPos 左下角(-256,-256)右上角(256,256) 右下角(256,-256)
            float pixelWidth = drawSprite.rect.width;//512
            float pixelHeight = drawSprite.rect.height;//512

            float centeredX = localPos.x + pixelWidth / 2;
            float centeredY = localPos.y + pixelHeight / 2;
            //左下角(0,0)右上角(512,512)
            Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centeredX), Mathf.RoundToInt(centeredY));
            // Debug.Log(pixel_pos);
            return pixel_pos;
        }
        #endregion

        #region 生成笔刷
        private void CreateAndSaveSoftBrushTexture(int size, string filePath)
        {
            // 创建一个新的Texture2D
            Texture2D brushTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;

            // 创建一个颜色数组来存储每个像素的颜色
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 计算当前像素到中心的距离
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float normalizedDistance = distance / center;
                    // 计算透明度，距离中心越远透明度越高
                    float alpha = Mathf.Clamp01(1 - normalizedDistance);
                    alpha = Mathf.SmoothStep(0, 1, alpha);

                    // 设置像素颜色，颜色为白色，透明度根据距离计算
                    pixels[y * size + x] = new Color(1, 1, 1, alpha);
                }
            }

            // 将颜色数组应用到Texture2D
            brushTexture.SetPixels(pixels);
            brushTexture.Apply();

            // 将Texture2D编码为PNG
            byte[] bytes = brushTexture.EncodeToPNG();

            // 保存PNG文件
            File.WriteAllBytes(filePath, bytes);

            Debug.Log($"软笔刷贴图已保存为: {filePath}");
        }
        #endregion

        /// <summary>
        /// 还原画板
        /// </summary>
        public void ResetBoard()
        {
            Debug.Log("[[MRDrawingScratch]ResetBoard");
            currentColorArray = new Color32[orignalColorArray.Length];
            Array.Copy(orignalColorArray, currentColorArray, orignalColorArray.Length);
            drawableTexture2D.SetPixels32(currentColorArray);
            drawableTexture2D.Apply();
            savePixelStack.Clear();
            revertPixelStack.Clear();
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        public void SaveImage()
        {
            byte[] bytes = drawableTexture2D.EncodeToPNG();
            string savePath = Application.dataPath + "/SavedScreen.png";
            Debug.Log($"[MRDrawingScratch]保存图片到：{savePath}");
            File.WriteAllBytes(savePath, bytes);
        }

        /// <summary>
        /// 回撤一步
        /// </summary>
        public void BackStep()
        {
            Debug.Log($"[MRDrawingScratch]BackStep");
            if (savePixelStack.Count > 0)
            {
                previousColorArray = savePixelStack.Pop();
                revertPixelStack.Push(previousColorArray);

                // 找到上一次的颜色数组
                if (savePixelStack.Count == 0)
                {
                    currentColorArray = new Color32[orignalColorArray.Length];
                    Array.Copy(orignalColorArray, currentColorArray, orignalColorArray.Length);
                    drawableTexture2D.SetPixels32(orignalColorArray);
                    drawableTexture2D.Apply();
                }
                else
                {
                    currentColorArray = savePixelStack.Peek();
                    drawableTexture2D.SetPixels32(currentColorArray);
                    drawableTexture2D.Apply();
                }
            }
            else
            {
                Debug.Log("没有可以回撤的步骤了");
            }
        }
        /// <summary>
        /// 恢复一步
        /// </summary>
        public void RevertStep()
        {
            Debug.Log($"[MRDrawingScratch]RevertStep");
            if (revertPixelStack.Count > 0)
            {
                // 可以恢复
                currentColorArray = revertPixelStack.Pop();
                drawableTexture2D.SetPixels32(currentColorArray);
                drawableTexture2D.Apply();
                savePixelStack.Push(currentColorArray);
            }
            else
            {
                Debug.Log("没有可以恢复的步骤了");
            }
        }

        /// <summary>
        /// 改变画笔颜色，黑色-笔、白色-橡皮、透明-涂抹
        /// </summary>
        /// <param name="_color"></param>
        public void ChangeMode(DrawingMode mode)
        {
            mMode = mode;
            if (mode == DrawingMode.Pen)
            {
                penColour = Color.black;
            }
            else if (mode == DrawingMode.Eraser)
            {
                penColour = Color.white;
            }
            else if (mode == DrawingMode.Smear)
            {
                penColour = Color.clear;
            }
        }

        private void StartDraw()
        {
            Debug.Log("[MRDrawingScratch]StartDraw");
            // 开始绘画
            currentColorArray = drawableTexture2D.GetPixels32();
            isDraging = true;
        }
        private void StopDraw()
        {
            Debug.Log($"[MRDrawingScratch]StopDraw");
            CalculateScratchProcess();
            // 结束绘画
            isDraging = false;
            previousDragPosition = Vector2.zero;
            previousColorArray = drawableTexture2D.GetPixels32();
            savePixelStack.Push(previousColorArray);
            revertPixelStack.Clear();
        }

        private void CalculateScratchProcess()
        {
            if (mMode == DrawingMode.Smear)
            {
                int mScratchindex = 0;
                // 计算擦除进度
                for (int i = 0; i < currentColorArray.Length; i++)
                {
                    if (currentColorArray[i].r > 128 && currentColorArray[i].g > 128 && currentColorArray[i].b > 128)
                    {
                        //Debug.Log($"[MRDrawingScratch]当前颜色：{currentColorArray[i]}");
                        mScratchindex++;
                    }
                }
                mScratchProcess = (float)mScratchindex / currentColorArray.Length;
                Debug.Log($"[MRDrawingScratch]擦除进度：{mScratchindex},{mScratchProcess}");
            }
        }

        #region 接口
        public virtual void OnFocusEnter(FocusEventData eventData)
        {
            // Hololens或Rokid
#if UNITY_WSA || UNITY_ANDROID
            //Debug.Log($"[MRDrawingScratch]焦点进入{this.name}");
            mIsEnterBoard = true;
#endif
        }

        public virtual void OnFocusExit(FocusEventData eventData)
        {
            // Hololens或Rokid
#if UNITY_WSA || UNITY_ANDROID
            //Debug.Log($"[MRDrawingScratch]焦点退出{this.name}");
            mIsEnterBoard = false;
            if (isDraging)
            {
                // 结束绘画
                StopDraw();
            }
#endif
        }

        public void OnInputUp(InputEventData eventData)
        {
            // Hololens或Rokid
#if UNITY_WSA || UNITY_ANDROID
            //Debug.Log($"[MRDrawingScratch]手指打开>>{this.name}");
            mIsPinching = false;
            mCurPointer = null;
            if (isDraging)
            {
                // 结束绘画
                StopDraw();
            }
#endif
        }

        public void OnInputDown(InputEventData eventData)
        {
            // Hololens或Rokid
#if UNITY_WSA || UNITY_ANDROID
            //Debug.Log($"[MRDrawingScratch]手指捏合>>{this.name}");
            mIsPinching = true;
            mCurPointer = eventData.InputSource.Pointers[0];
            if (!isDraging && mIsEnterBoard)
            {
                // 开始绘画
                StartDraw();
            }
#endif
        }
#endregion
    }
}
