﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO.Ports;

namespace PreMaid
{
    /// <summary>
    /// モーションデータをベタで送るテスト
    /// </summary>
    public class MotionDataWriterSample : MonoBehaviour
    {
        /// <summary>
        /// ポーズ終了（ゆっくりTポーズを取り、待機ポーズになる）
        /// </summary>
        private string forceTpose =
            "04 05 00 01";

        /// <summary>
        /// 「どうぞ」というポーズを取る命令
        /// </summary>
        private string poseDouzo =
            "05 1F 00 3E 24";
        
        /// <summary>
        /// 格納しているダンスを踊る命令
        /// </summary>
        private string startDance =
            "05 1F 00 01 1B";

        private string portName = "COM7";
        private int baudRate    = 115200;
        private SerialPort serialPort_;
     
        
        
        //適当に抜き出したポーズ
        //まだ動作確認出来ていない
        private string testPose =
            "50 18 00 06 02 5F 1D 03 4C 1D 04 D5 23 05 4C 1D 06 4C 1D 07 4C 1D 08 4C 1D 09 CE 24 0A A2 1C 0B 44 14 0C F6 1D 0D CF 13 0E E4 1B 0F 9B 25 10 B4 1E 11 47 1D 12 7C 1A 13 A6 0F 14 1C 20 15 04 2B 16 E4 1B 17 D6 13 18 B4 1E 1A 3C 1E 1C 5C 1C AF";


        [SerializeField]
        private int defaultHeadYaw = 7500;
        
        
        // Start is called before the first frame update
        void Start()
        {
            //存在しないシリアルポートにはアクセスしないように
            var portNames = SerialPort.GetPortNames();

            foreach (var VARIABLE in portNames)
            {
                Debug.Log(VARIABLE);
            }
            
            if (portNames.Contains(portName))
            {
                try
                {
                    serialPort_ = new SerialPort(" \\\\.\\"+portName, baudRate, Parity.None, 8, StopBits.One);
                    serialPort_.Open();
                    Debug.Log("シリアルポート:" + serialPort_.PortName + " 接続成功");
                }
                catch (Exception e)
                {
                    Debug.LogWarning("シリアルポートOpen失敗しました、ペアリング済みか、プリメイドAIのポートか確認してください");
                    Console.WriteLine(e);
                    //throw;
                }
                
            }
            else
            {
                Debug.LogWarning($"指定された{portName}がありません。portNameを書き換えてください");
            }
        }

        // Update is called once per frame
        void Update()
        {

            //動作確認にもTポーズというかポーズ終了を送るのは良いこと
            if (Input.GetKeyDown(KeyCode.A))
            {
                byte[] data = PreMaidUtility.BuildByteDataFromStringOrder(forceTpose);
                serialPort_.Write(data, 0, data.Length);
                
                PreMaidUtility.DumpDebugLogToHex(data);
                
            }

            //どうぞ、のポーズを取る
            if (Input.GetKeyDown(KeyCode.B))
            {
                byte[] data = PreMaidUtility.BuildByteDataFromStringOrder(poseDouzo);
                serialPort_.Write(data, 0, data.Length);
                
                PreMaidUtility.DumpDebugLogToHex(data);
            }

            //格納しているダンスモーションを再生する
            if (Input.GetKeyDown(KeyCode.C))
            {
                byte[] data = PreMaidUtility.BuildByteDataFromStringOrder(startDance);
                serialPort_.Write(data, 0, data.Length);
                
                PreMaidUtility.DumpDebugLogToHex(data);
            }

            //まだ動作していない、任意サーボ情報を送るポーズ指定
            if (Input.GetKeyDown(KeyCode.D))
            {
                byte[] data = PreMaidUtility.BuildByteDataFromStringOrder(testPose);
                serialPort_.Write(data, 0, data.Length);

                PreMaidUtility.DumpDebugLogToHex(data);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                serialPort_.Close();
            }

            //指定の1フレームポーズの転送
            if (Input.GetKeyDown(KeyCode.K))
            {
                StartCoroutine(TestPosePlay());
                //Debug.Log(CalcXorString("05 1F 00 01"));
                //Debug.Log(RewriteXorString("05 1F 00 01 FF"));
            }
            
            //指定の1フレームポーズの転送
            if (Input.GetKeyDown(KeyCode.E))
            {
                defaultHeadYaw = 7500;
                StartCoroutine(MoveLeftShoulderRotatePose(defaultHeadYaw));
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                defaultHeadYaw -= 500;
                StartCoroutine(MoveLeftShoulderRotatePose(defaultHeadYaw));
            }
            
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                defaultHeadYaw += 500;
                StartCoroutine(MoveLeftShoulderRotatePose(defaultHeadYaw));
            }
        }


        /// <summary>
        /// その場でダンスモーション相当を生成して転送して再生、という例
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public IEnumerator MoveLeftShoulderRotatePose(int target)
        {
            //サーボリミットは3000から11000くらいという話もあるけど、安全のためそれよりチェックは厳しめにしておく
            if (target < 4000 || target > 10000)
            {
                yield break;
            }
            
            //04番=L肩ピッチのサーボの値をtargetのサーボの値に書き換えて、えいって送る
            var bytes = BitConverter.GetBytes(target);
            string str= string.Empty;
            for (int i = 0; i < 2; i++) {
                str += string.Format("{0:X2} ", bytes[i]);
            }

            var pose = "50 18 00 06 02 4C 1D 03 4C 1D 04 " + str +
                       "05 4C 1D 06 4C 1D 07 4C 1D 08 4C 1D 09 1C 25 0A 4C 1D 0B 7C 15 0C 4C 1D 0D 4C 1D 0E 4C 1D 0F 4C 1D 10 4C 1D 11 4C 1D 12 4C 1D 13 4C 1D 14 4C 1D 15 4C 1D 16 4C 1D 17 4C 1D 18 4C 1D 1A 4C 1D 1C 4C 1D D9";
            
            Debug.Log(pose);
            
            
            //仮でwaitは0.05秒*12 = 600ms掛かっているけど、0.01秒刻みだと転送間に合わないことがあった
            //応答性を上げるにはwait設定を詰めたり、ベリファイダンプをしないようにしたり、いろいろな手がありそう～！
            //直感的には3FPSくらいまではツメられそう。
            float waitSec = 0.05f;
            
            byte[] data1 = PreMaidUtility.BuildByteDataFromStringOrder("07 01 00 02 00 02 06");
            serialPort_.Write(data1, 0, data1.Length);
            yield return new WaitForSeconds(waitSec);
            
            byte[] data2 = PreMaidUtility.BuildByteDataFromStringOrder("07 01 00 08 00 02 0C");
            serialPort_.Write(data2, 0, data2.Length);
            yield return new WaitForSeconds(waitSec);
              
            byte[] data3 = PreMaidUtility.BuildByteDataFromStringOrder("08 02 00 08 00 FF FF 02");
            serialPort_.Write(data3, 0, data3.Length);
            yield return new WaitForSeconds(waitSec);
            
            byte[] data4 = PreMaidUtility.BuildByteDataFromStringOrder("04 04 00 00");//フラッシュのライトプロテクト解除？
            serialPort_.Write(data4, 0, data4.Length);
            yield return new WaitForSeconds(waitSec);
            
            byte[] data5 = PreMaidUtility.BuildByteDataFromStringOrder("5c 1d 00 00 00"); //転送コマンド？
            serialPort_.Write(data5, 0, data5.Length);
            yield return new WaitForSeconds(waitSec);
            
            byte[] data6 = PreMaidUtility.BuildByteDataFromStringOrder(PreMaidUtility.RewriteXorString(pose));    //対象のモーション、今回は1個だけ
            serialPort_.Write(data6, 0, data6.Length);
            yield return new WaitForSeconds(waitSec);
            
            byte[] data7 = PreMaidUtility.BuildByteDataFromStringOrder("04 17 00 13 ff ff 41"); //不明
            serialPort_.Write(data7, 0, data7.Length);
            yield return new WaitForSeconds(waitSec);
            
            byte[] data8 = PreMaidUtility.BuildByteDataFromStringOrder("05 1E 00 01 1A");
            serialPort_.Write(data8, 0, data8.Length);
            yield return new WaitForSeconds(waitSec);

            byte[] data9 = PreMaidUtility.BuildByteDataFromStringOrder("05 1C 00 01 18"); //ベリファイダンプ要請
            serialPort_.Write(data9, 0, data9.Length);
            yield return new WaitForSeconds(waitSec);
            
            byte[] data10 = PreMaidUtility.BuildByteDataFromStringOrder("08 02 00 08 00 08 00 0A");//モーションデータ転送終了
            serialPort_.Write(data10, 0, data10.Length);
            yield return new WaitForSeconds(waitSec);

            byte[] data11 = PreMaidUtility.BuildByteDataFromStringOrder("04 04 00 00");//フラッシュのライトプロテクトを掛ける？
            serialPort_.Write(data11, 0, data11.Length);
            yield return new WaitForSeconds(waitSec);

            byte[] data12 = PreMaidUtility.BuildByteDataFromStringOrder("05 1F 00 01 1B");//01番モーション再生
            serialPort_.Write(data12, 0, data12.Length);
            yield return new WaitForSeconds(waitSec);
        }
        
        /// <summary>
        /// 無理やり1フレームだけのダンスモーションを転送して再生する
        /// </summary>
        /// <returns></returns>
        public IEnumerator TestPosePlay()
        {

            float waitSec = 0.05f;
            
            byte[] data1 = PreMaidUtility.BuildByteDataFromStringOrder("07 01 00 02 00 02 06");
            serialPort_.Write(data1, 0, data1.Length);
            yield return new WaitForSeconds(waitSec);
            
            byte[] data2 = PreMaidUtility.BuildByteDataFromStringOrder("07 01 00 08 00 02 0C");
            serialPort_.Write(data2, 0, data2.Length);
            yield return new WaitForSeconds(waitSec);
              
            byte[] data3 = PreMaidUtility.BuildByteDataFromStringOrder("08 02 00 08 00 FF FF 02");
            serialPort_.Write(data3, 0, data3.Length);
            yield return new WaitForSeconds(waitSec);
            
            byte[] data4 = PreMaidUtility.BuildByteDataFromStringOrder("04 04 00 00");//フラッシュのライトプロテクト解除？
            serialPort_.Write(data4, 0, data4.Length);
            yield return new WaitForSeconds(waitSec);
            
            byte[] data5 = PreMaidUtility.BuildByteDataFromStringOrder("5c 1d 00 00 00"); //転送コマンド？
            serialPort_.Write(data5, 0, data5.Length);
            yield return new WaitForSeconds(waitSec);
            
            byte[] data6 = PreMaidUtility.BuildByteDataFromStringOrder(PreMaidUtility.RewriteXorString(testPose));    //対象のモーション、今回は1個だけ
            serialPort_.Write(data6, 0, data6.Length);
            yield return new WaitForSeconds(waitSec);
            
            byte[] data7 = PreMaidUtility.BuildByteDataFromStringOrder("04 17 00 13 ff ff 41"); //不明
            serialPort_.Write(data7, 0, data7.Length);
            yield return new WaitForSeconds(waitSec);
            
            byte[] data8 = PreMaidUtility.BuildByteDataFromStringOrder("05 1E 00 01 1A");
            serialPort_.Write(data8, 0, data8.Length);
            yield return new WaitForSeconds(waitSec);

            byte[] data9 = PreMaidUtility.BuildByteDataFromStringOrder("05 1C 00 01 18"); //ベリファイダンプ要請
            serialPort_.Write(data9, 0, data9.Length);
            yield return new WaitForSeconds(waitSec);
            
            byte[] data10 = PreMaidUtility.BuildByteDataFromStringOrder("08 02 00 08 00 08 00 0A");//モーションデータ転送終了
            serialPort_.Write(data10, 0, data10.Length);
            yield return new WaitForSeconds(waitSec);

            byte[] data11 = PreMaidUtility.BuildByteDataFromStringOrder("04 04 00 00");//フラッシュのライトプロテクトを掛ける？
            serialPort_.Write(data11, 0, data11.Length);
            yield return new WaitForSeconds(waitSec);

            byte[] data12 = PreMaidUtility.BuildByteDataFromStringOrder("05 1F 00 01 1B");//01番モーション再生
            serialPort_.Write(data12, 0, data12.Length);
            yield return new WaitForSeconds(waitSec);

        }
        
        
       
        
       
    }
}