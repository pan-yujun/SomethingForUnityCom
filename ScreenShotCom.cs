﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace IndieStudio.DrawingAndColoring.Utility
{
    public class ScreenShotCom : MonoBehaviour
    {
        public Camera mMianCamera;
        public Camera mMiddleCamera;
        public RectTransform mRect;// 矩形截图区域

        private string m_screenShotPath;
        //private Action<Texture2D> m_screenshotTextureAction;
        //private Action<string> m_screenshotPathAction;

        private void Awake()
        {
            m_screenShotPath = $"{Application.temporaryCachePath}/DrawScreenShot.png";
            if (mMiddleCamera == null)
            {
                mMiddleCamera = GameObject.Find("MiddleCamera").GetComponent<Camera>();
            }
        }
        /// <summary>
        /// 截取绘画区域
        /// </summary>
        /// <param name="textureAction"></param>
        public void TakeMiddleCameraScreenshot(Action<Texture2D> textureAction = null, Action<string> pathAction = null)
        {
            StartCoroutine(DoMiddleCameraScreenshot(true, textureAction, pathAction));
        }

        /// <summary>
        /// 截取整个绘画面板
        /// </summary>
        /// <param name="textureAction"></param>
        public void TakeAllCameraScreenshot(Action<Texture2D> textureAction = null)
        {
            StartCoroutine(DoAllCameraScreenshot(textureAction));
        }

        /// <summary>
        /// 截取相机的rt
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoMiddleCameraScreenshot(bool ridBlankPixel, Action<Texture2D> textureAction = null, Action<string> pathAction = null)
        {
            yield return new WaitForEndOfFrame();

            // 创建一个RenderTexture对象
            RenderTexture rt = new RenderTexture((int)Screen.width, (int)Screen.height, 0);
            // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机
            mMiddleCamera.targetTexture = rt;
            mMiddleCamera.Render();

            RenderTexture.active = rt;
            Rect rect = new Rect(0, 0, Screen.width, Screen.height);
            Texture2D fullScreenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
            fullScreenShot.ReadPixels(rect, 0, 0);
            fullScreenShot.Apply();

            // 重置相关参数，以使用camera继续在屏幕上显示
            mMiddleCamera.targetTexture = null;
            RenderTexture.active = null; 
            Destroy(rt);

            Texture2D screenShot = fullScreenShot;
            if (ridBlankPixel)
            {
                // 处理边缘透明像素
                screenShot = ClipBlank(fullScreenShot);
                Destroy(fullScreenShot);
            }
            

            //保存截图
            byte[] byteArray = screenShot.EncodeToPNG();
            File.WriteAllBytes(m_screenShotPath, byteArray);

            Debug.Log($"DoCameraScreenshot截图完成");

            if (textureAction != null)
            {
                textureAction?.Invoke(screenShot);
            }
            else
            {
                // 清除Texture
                Destroy(screenShot);
            }

            pathAction?.Invoke(m_screenShotPath);
        }

        /// <summary>
        /// 删除RT边缘透明像素
        /// </summary>
        /// <param name="orgin"></param>
        /// <returns></returns>
        private Texture2D ClipBlank(Texture2D orgin)
        {
            try
            {
                var left = 0;
                var top = 0;
                var right = orgin.width;
                var botton = orgin.height;

                // 左侧
                for (var i = 0; i < orgin.width; i++)
                {
                    var find = false;
                    for (var j = 0; j < orgin.height; j++)
                    {
                        var color = orgin.GetPixel(i, j);
                        if (Math.Abs(color.a) > 1e-6)
                        {
                            find = true;
                            break;
                        }
                    }
                    if (find)
                    {
                        left = i;
                        break;
                    }
                }

                // 右侧
                for (var i = orgin.width - 1; i >= 0; i--)
                {
                    var find = false;
                    for (var j = 0; j < orgin.height; j++)
                    {
                        var color = orgin.GetPixel(i, j);
                        if (Math.Abs(color.a) > 1e-6)
                        {
                            find = true;
                            break;
                        }
                    }
                    if (find)
                    {
                        right = i;
                        break;
                    }
                }

                // 上侧
                for (var j = 0; j < orgin.height; j++)
                {
                    var find = false;
                    for (var i = 0; i < orgin.width; i++)
                    {
                        var color = orgin.GetPixel(i, j);
                        if (Math.Abs(color.a) > 1e-6)
                        {
                            find = true;
                            break;
                        }
                    }
                    if (find)
                    {
                        top = j;
                        break;
                    }
                }

                // 下侧
                for (var j = orgin.height - 1; j >= 0; j--)
                {
                    var find = false;
                    for (var i = 0; i < orgin.width; i++)
                    {
                        var color = orgin.GetPixel(i, j);
                        if (Math.Abs(color.a) > 1e-6)
                        {
                            find = true;
                            break;
                        }
                    }
                    if (find)
                    {
                        botton = j;
                        break;
                    }
                }

                // 创建新纹理
                var width = right - left;
                var height = botton - top;

                var result = new Texture2D(width, height, TextureFormat.ARGB32, false);

                // 复制有效颜色区块
                var colors = orgin.GetPixels(left, top, width, height);
                result.SetPixels(0, 0, width, height, colors);

                result.Apply();
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// 截取整个绘画面板
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoAllCameraScreenshot(Action<Texture2D> textureAction = null)
        {
            yield return new WaitForEndOfFrame();

            // 创建一个RenderTexture对象
            RenderTexture rt = new RenderTexture((int)Screen.width, (int)Screen.height, 0);
            // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机
            mMiddleCamera.targetTexture = rt;
            mMiddleCamera.Render();
            mMianCamera.targetTexture = rt;
            mMianCamera.Render();

            RenderTexture.active = rt;
            Rect rect = new Rect(0, 0, Screen.width, Screen.height);
            Texture2D fullScreenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
            fullScreenShot.ReadPixels(rect, 0, 0);
            fullScreenShot.Apply();

            // 重置相关参数，以使用camera继续在屏幕上显示
            mMiddleCamera.targetTexture = null;
            mMianCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);

            //测试
            //byte[] byteArray = fullScreenShot.EncodeToPNG();
            //File.WriteAllBytes("D:/FullDrawScreenShot.png", byteArray);

            textureAction?.Invoke(fullScreenShot);
        }
    }
}
