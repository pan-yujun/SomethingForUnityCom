package com.ffmpeg.unity.bind2;

//https://stackoverflow.com/a/45776104
//https://gamedev.stackexchange.com/a/185023
import com.unity3d.player.UnityPlayer;

public class CallbackNotifier {

   public static void onStart(long executionId) {
      String messageText = toMessage("OnStart", executionId, "");
      sendCallback(messageText);
   }

   public static void onLog(long executionId, String text) {
      String messageText = toMessage("OnLog", executionId, text);
      sendCallback(messageText);
   }

   public static void onWarning(long executionId, String text) {
      String messageText = toMessage("OnWarning", executionId, text);
      sendCallback(messageText);
   }

   public static void onError(long executionId, String text) {
      String messageText = toMessage("OnError", executionId, text);
      sendCallback(messageText);
   }

   public static void onSuccess(long executionId) {
      String messageText = toMessage("OnSuccess", executionId, "");
      sendCallback(messageText);
   }

   public static void onCanceled(long executionId) {
      String messageText = toMessage("OnCanceled", executionId, "");
      sendCallback(messageText);
   }

   public static void onFail(long executionId) {
      String messageText = toMessage("OnFail", executionId, "");
      sendCallback(messageText);
   }

   static String toMessage(String eventType, long executionId, String text) {
      return eventType + "|" + executionId + "|" + text;
   }

   static void sendCallback(String message) {
      UnityPlayer.UnitySendMessage("FFmpegMobileCallbacksHandler", "OnFFmpegMobileCallback", message);
   }
}

//public interface CallbackNotifier {
//
//   void onStart(long executionId);
//   void onLog(long executionId, String message);
//   void onWarning(long executionId, String message);
//   void onError(long executionId, String message);
//   void onSuccess(long executionId);
//   void onCanceled(long executionId);
//   void onFail(long executionId);
//}