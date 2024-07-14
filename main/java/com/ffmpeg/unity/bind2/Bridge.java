package com.ffmpeg.unity.bind2;

import com.arthenica.mobileffmpeg.Signal;

import android.util.Log;

import com.arthenica.mobileffmpeg.Config;
import com.arthenica.mobileffmpeg.ExecuteCallback;
import com.arthenica.mobileffmpeg.FFmpeg;
import com.arthenica.mobileffmpeg.LogCallback;
import com.arthenica.mobileffmpeg.LogMessage;

import static com.arthenica.mobileffmpeg.Config.RETURN_CODE_CANCEL;
import static com.arthenica.mobileffmpeg.Config.RETURN_CODE_SUCCESS;
import static com.arthenica.mobileffmpeg.Level.AV_LOG_INFO;
import static com.arthenica.mobileffmpeg.Level.AV_LOG_WARNING;

public class Bridge {

    //CallbackNotifier:AndroidJavaProxy makes Stack Overflow and crashes.
    //That is why Unity Send Message is used instead.
    public static long execute(String command/*, final CallbackNotifier unityCallbackInterface*/) {

        //https://github.com/tanersener/mobile-ffmpeg/issues/258#issuecomment-645664260
        Config.ignoreSignal(Signal.SIGXCPU);

        final long executionId = FFmpeg.executeAsync(command, new ExecuteCallback() {

            @Override
            public void apply(final long executionId, final int returnCode) {
                if (returnCode == RETURN_CODE_SUCCESS) {
                    //Log.d("!!!", "RAW onSuccess: " + executionId);
                    CallbackNotifier.onSuccess(executionId);
                    //unityCallbackInterface.onSuccess(executionId);
                } else if (returnCode == RETURN_CODE_CANCEL) {
                    //Log.d("!!!", "RAW onCanceled: " + executionId);
                    //unityCallbackInterface.onCanceled(executionId);
                    CallbackNotifier.onCanceled(executionId);
                } else {
                    //Log.d("!!!", "RAW onFail: " + executionId);
                    CallbackNotifier.onFail(executionId);
                    //unityCallbackInterface.onFail(executionId);
                }
            }
        });

        Config.enableLogCallback(new LogCallback() {
            public void apply(LogMessage message) {
                if(message.getLevel().getValue() >= AV_LOG_INFO.getValue()) {
                    //Log.d("!!!", "RAW onLog: " + executionId + ", message: " + message.getText());
                    CallbackNotifier.onLog(executionId, message.getText());
                    //unityCallbackInterface.onLog(executionId, message.getText());
                } else if(message.getLevel().getValue() >= AV_LOG_WARNING.getValue()) {
                    //Log.d("!!!", "RAW onWarning: " + executionId + ", message: " + message.getText());
                    //unityCallbackInterface.onWarning(executionId, message.getText());
                    CallbackNotifier.onWarning(executionId, message.getText());
                } else {
                    //Log.d("!!!", "RAW onError: " + executionId + ", message: " + message.getText());
                    //unityCallbackInterface.onError(executionId, message.getText());
                    CallbackNotifier.onError(executionId, message.getText());
                }
            }
        });

        //Log.d("!!!", "RAW onStart: " + executionId);
        CallbackNotifier.onStart(executionId);
        //unityCallbackInterface.onStart(executionId);

        return executionId;
    }

    public static void cancel(long executionId) {

        FFmpeg.cancel(executionId);
    }
}