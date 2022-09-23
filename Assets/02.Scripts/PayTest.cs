using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

//public enum DealType
//{
//    신용승인,
//    신용취소//,
//    //QR페이승인,
//    //QR페이취소
//}

public enum DealType
{
    CREDIT_APPROVAL,
    CREDIT_CANCELLATION,
    //QR_APPROVAL,
    //QR_CANCELLATION,
    KAKAO_APPROVAL,
    KAKAO_CANCELLATION,
    NAVER_APPROVAL,
    NAVER_CANCELLATION,
    ZERO_APPROVAL,
    ZERO_CANCELLATION
}

/// <summary>
/// 거래구분 : 0200 (승인)
///	거래유형 : 10(신용)
/// WCC : C(카드)
/// 거래금액 : 1004
/// 할부 00(일시불)
/// 
/// 거래구분 : 0300 (승인)
/// 거래유형 : 10 (카카오페이/머니/알리위챗/QR)
/// WCC : G (QRIC)
/// 거래금액 : 1004
/// 할부 : 00 (일시불)
/// </summary>

public class PayTest : MonoBehaviour
{
    private DealType dealType;
    [SerializeField] private UDP_Server UDP_Server;

    private int payValue;
    public int PayValue { get { return payValue; } set { payValue = value; } }

    /// <summary>
    /// NVCAT 승인 전문 송수신
    /// </summary>
    /// <param name="SendBuf">INPUT / NVCAT 요청전문</param>
    /// <param name="RecvBuf">OUTPUT / NVCAT 응답전문</param>
    [DllImport("NVCAT", CharSet = CharSet.Unicode)]
    public static extern int NICEVCAT(byte[] SendBuf, byte[] RecvBuf);

    /// <summary>
    /// NVCAT 바코드/PAYPRO 승인 전문 송수신
    /// </summary>
    /// <param name="SendBuf">INPUT / NVCAT 요청전문</param>
    /// <param name="RecvBuf">OUTPUT / NVCAT 응답전문</param>
    [DllImport("NVCAT", CharSet = CharSet.Unicode)]
    public static extern int NICEVCATB(byte[] SendBuf, byte[] RecvBuf);

    /// <summary>
    /// NVCAT 바코드 요청
    /// </summary>
    /// <param name="SendBuf">INPUT / 장비 TYPE</param>
    /// <param name="RecvBuf">OUTPUT / 바코드(QR) 데이터 </param>
    [DllImport("NVCAT", CharSet = CharSet.Unicode)]
    public static extern int REQ_BARCODE(byte[] SendBuf, byte[] RecvBuf);

    /// <summary>
    /// 카드리딩 요청 취소
    /// </summary>
    [DllImport("NVCAT", CharSet = CharSet.Unicode)]
    public static extern int REQ_STOP();

    /// <summary>
    /// NVCAT 데몬 프로그램 재시작 (미 구동시 실행)
    /// </summary>
    [DllImport("NVCAT", CharSet = CharSet.Unicode)]
    public static extern int RESTART();

    /// <summary>
    /// NVCAT 강제 종료
    /// </summary>
    [DllImport("NVCAT", CharSet = CharSet.Unicode)]
    public static extern int NVCATSHUTDOWN();

    //[DllImport("NVCAT.dll", CharSet = CharSet.Unicode)]
    //public static extern int READER_RESET(byte[] time);

    // -----------------------------------------------------------------------------------------------------

    /*
    public void PayEvent()
    {
        PayValue = DataManager.instance.payValue;
        Debug.Log(string.Format("페이 테스트에서 페이 이벤트 함수 안에서 Pay Value : {0}", payValue));

        string FS = ((char)28).ToString(); // 전문 구분자 (File Separator)
        string SendData = string.Empty;
        string num = string.Empty;
        string str = string.Empty;

        ///Debug.Log(string.Format("결제 DealType : {0}", dealType.ToString()));

        switch (dealType)
        {
            case DealType.CREDIT_APPROVAL:
                Debug.Log("신용승인");
                SendData = "0200" + FS + "10" + FS + "C" + FS + PayValue.ToString() + FS + FS + FS + "00" + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS;
                break;

            case DealType.CREDIT_CANCELLATION:
                Debug.Log("신용취소");
                SendData = "0420" + FS + "10" + FS + "C" + FS + PayValue.ToString() + FS + FS + FS + "00" + FS + FS + FS + "28713392" + FS + "220214" + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS;
                break;

                //case DealType.QR페이승인:
                //    Debug.Log("QR페이승인");
                //    SendData = "0300" + FS + "10" + FS + "G" + FS + PayValue.ToString() + FS + FS + FS + "00" + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS;
                //    break;

                //case DealType.QR페이취소:
                //    Debug.Log("QR페이취소");
                //    SendData = "0520" + FS + "10" + FS + "G" + FS + PayValue.ToString() + FS + FS + FS + "00" + FS + FS + FS + "28713392" + FS + "220214" + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS;
                //    break;
        }

        //Debug.Log(SendData);

        byte[] mSend = Encoding.GetEncoding(1252).GetBytes(SendData); // 요청전문 (1004원 일시불 신용승인 전문) Byte Array 저장

        //Debug.Log(mSend.Length);

        byte[] mRecv = new byte[2048]; // 응답전문 Byte Array

        //for(int i = 0; i < mSend.Length; i++)
        //{
        //    Debug.Log(mSend[i]);
        //}
        int ret = NICEVCAT(mSend, mRecv); // 함수호출

        Debug.Log(ret.ToString());

        Debug.Log(Encoding.Default.GetString(mRecv)); // 응답전문 String

        if (ret.Equals(1))
        {
            // 리턴값 1인 경우만 응답전문이 정상 수신
            // 리턴값 1이면서 응답코드 “0000”인 경우만 정상 승인
            // 리턴값 1인 경우 응답전문 수신됨

            int i = 0;
            int j = 0;
            int k = 0;

            string recvdata = Encoding.Default.GetString(mRecv); // 응답전문 recvdata에 저장

            while (true)
            {
                if (recvdata.Substring(i, 1).Equals(FS))
                {
                    j++;

                    switch (j)
                    {
                        case 1:
                            Debug.Log(string.Format("거래구분 : {0}", recvdata.Substring(k, i - k))); //거래구분
                            break;
                        case 2:
                            Debug.Log(string.Format("거래유형 : {0}", recvdata.Substring(k, i - k))); //거래유형
                            break;
                        case 3:
                            Debug.Log(string.Format("응답코드 : {0}", recvdata.Substring(k, i - k))); //응답코드
                            break;
                        case 4:
                            Debug.Log(string.Format("거래금액 : {0}", recvdata.Substring(k, i - k))); //거래금액
                            break;
                        case 5:
                            Debug.Log(string.Format("부가세 : {0}", recvdata.Substring(k, i - k))); //부가세
                            break;
                        case 6:
                            Debug.Log(string.Format("봉사료 : {0}", recvdata.Substring(k, i - k))); //봉사료
                            break;
                        case 7:
                            Debug.Log(string.Format("할부개월 : {0}", recvdata.Substring(k, i - k))); //할부개월
                            break;
                        case 8:
                            Debug.Log(string.Format("승인번호 : {0}", recvdata.Substring(k, i - k))); //승인번호
                            num = recvdata.Substring(k, i - k);
                            break;
                        case 9:
                            Debug.Log(string.Format("승인일시 : {0}", recvdata.Substring(k, i - k))); //승인일시
                            break;
                        case 10:
                            Debug.Log(string.Format("발급사코드 : {0}", recvdata.Substring(k, i - k))); //발급사코드
                            break;
                        case 11:
                            Debug.Log(string.Format("발급사명 : {0}", recvdata.Substring(k, i - k))); //발급사명
                            break;
                        case 12:
                            Debug.Log(string.Format("매입사코드 : {0}", recvdata.Substring(k, i - k))); //매입사코드
                            break;
                        case 13:
                            Debug.Log(string.Format("매입사명 : {0}", recvdata.Substring(k, i - k))); //매입사명
                            break;
                        case 14:
                            Debug.Log(string.Format("가맹점번호 : {0}", recvdata.Substring(k, i - k))); //가맹점번호
                            break;
                        case 15:
                            Debug.Log(string.Format("승인CATID : {0}", recvdata.Substring(k, i - k))); //승인CATID
                            break;
                        case 16:
                            Debug.Log(string.Format("잔액 : {0}", recvdata.Substring(k, i - k))); //잔액
                            break;
                        case 17:
                            Debug.Log(string.Format("응답메시지 : {0}", recvdata.Substring(k, i - k))); //응답메시지
                            str = recvdata.Substring(k, i - k);
                            break;
                        case 18:
                            Debug.Log(string.Format("카드BIN : {0}", recvdata.Substring(k, i - k))); //카드BIN
                            break;
                        case 19:
                            Debug.Log(string.Format("카드구분 : {0}", recvdata.Substring(k, i - k))); //카드구분
                            break;
                        case 20:
                            Debug.Log(string.Format("전문관리번호 : {0}", recvdata.Substring(k, i - k))); //전문관리번호
                            break;
                        case 21:
                            Debug.Log(string.Format("거래일련번호 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                            break;
                    }
                    k = i + 1;

                    if (j.Equals(21))
                    {
                        break;
                    }
                }
                i++;
            }
        }

        //switch(str)
        //{
        //    case "정상승인":

        //        break;
        //}

        if (string.IsNullOrEmpty(num.Replace(" ", "")))
        {
            num = "null";
        }

        //Debug.Log(string.Format("{0},{1},{2}", ret, num, str));

        UDP_Server.SocketSend(string.Format("{0},{1},{2}", ret, num, str));
        UDP_Server._clientMessage = string.Empty;
    }
    */

    /// <summary>
    /// 신용카드 결세 only
    /// </summary>
    /// <param name="value">결제금액</param>
    public void PayEvent(int value)
    {
        PayValue = value;

        string FS = ((char)28).ToString(); // 전문 구분자 (File Separator)
        string SendData = string.Empty;
        string num = string.Empty;
        string str = string.Empty;

        //Debug.Log(string.Format("결제 DealType : {0}", dealType.ToString()));

        switch (dealType)
        {
            case DealType.CREDIT_APPROVAL:
                Debug.Log("신용승인");
                SendData = "0200" + FS + "10" + FS + "C" + FS + PayValue.ToString() + FS + FS + FS + "00" + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS;
                break;

            case DealType.CREDIT_CANCELLATION:
                Debug.Log("신용취소");
                SendData = "0420" + FS + "10" + FS + "C" + FS + PayValue.ToString() + FS + FS + FS + "00" + FS + FS + FS + "28713392" + FS + "220214" + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS;
                break;
        }


        Debug.Log(SendData);

        byte[] mSend = Encoding.GetEncoding(1252).GetBytes(SendData); // 요청전문 (1004원 일시불 신용승인 전문) Byte Array 저장

        //Debug.Log(mSend.Length);

        byte[] mRecv = new byte[2048]; // 응답전문 Byte Array

        //for(int i = 0; i < mSend.Length; i++)
        //{
        //    Debug.Log(mSend[i]);
        //}

        int ret = NICEVCAT(mSend, mRecv); // 함수호출

        //Debug.Log(Encoding.Default.GetString(mRecv)); // 응답전문 String

        if (ret.Equals(1))
        {
            // 리턴값 1인 경우만 응답전문이 정상 수신
            // 리턴값 1이면서 응답코드 “0000”인 경우만 정상 승인
            // 리턴값 1인 경우 응답전문 수신됨

            int i = 0;
            int j = 0;
            int k = 0;

            string recvdata = Encoding.Default.GetString(mRecv); // 응답전문 recvdata에 저장

            while (true)
            {
                if (recvdata.Substring(i, 1).Equals(FS))
                {
                    j++;

                    switch (j)
                    {
                        case 1:
                            Debug.Log(string.Format("거래구분 : {0}", recvdata.Substring(k, i - k))); //거래구분
                            break;
                        case 2:
                            Debug.Log(string.Format("거래유형 : {0}", recvdata.Substring(k, i - k))); //거래유형
                            break;
                        case 3:
                            Debug.Log(string.Format("응답코드 : {0}", recvdata.Substring(k, i - k))); //응답코드
                            break;
                        case 4:
                            Debug.Log(string.Format("거래금액 : {0}", recvdata.Substring(k, i - k))); //거래금액
                            break;
                        case 5:
                            Debug.Log(string.Format("부가세 : {0}", recvdata.Substring(k, i - k))); //부가세
                            break;
                        case 6:
                            Debug.Log(string.Format("봉사료 : {0}", recvdata.Substring(k, i - k))); //봉사료
                            break;
                        case 7:
                            Debug.Log(string.Format("할부개월 : {0}", recvdata.Substring(k, i - k))); //할부개월
                            break;
                        case 8:
                            Debug.Log(string.Format("승인번호 : {0}", recvdata.Substring(k, i - k))); //승인번호
                            num = recvdata.Substring(k, i - k);
                            break;
                        case 9:
                            Debug.Log(string.Format("승인일시 : {0}", recvdata.Substring(k, i - k))); //승인일시
                            break;
                        case 10:
                            Debug.Log(string.Format("발급사코드 : {0}", recvdata.Substring(k, i - k))); //발급사코드
                            break;
                        case 11:
                            Debug.Log(string.Format("발급사명 : {0}", recvdata.Substring(k, i - k))); //발급사명
                            break;
                        case 12:
                            Debug.Log(string.Format("매입사코드 : {0}", recvdata.Substring(k, i - k))); //매입사코드
                            break;
                        case 13:
                            Debug.Log(string.Format("매입사명 : {0}", recvdata.Substring(k, i - k))); //매입사명
                            break;
                        case 14:
                            Debug.Log(string.Format("가맹점번호 : {0}", recvdata.Substring(k, i - k))); //가맹점번호
                            break;
                        case 15:
                            Debug.Log(string.Format("승인CATID : {0}", recvdata.Substring(k, i - k))); //승인CATID
                            break;
                        case 16:
                            Debug.Log(string.Format("잔액 : {0}", recvdata.Substring(k, i - k))); //잔액
                            break;
                        case 17:
                            Debug.Log(string.Format("응답메시지 : {0}", recvdata.Substring(k, i - k))); //응답메시지
                            str = recvdata.Substring(k, i - k);
                            break;
                        case 18:
                            Debug.Log(string.Format("카드BIN : {0}", recvdata.Substring(k, i - k))); //카드BIN
                            break;
                        case 19:
                            Debug.Log(string.Format("카드구분 : {0}", recvdata.Substring(k, i - k))); //카드구분
                            break;
                        case 20:
                            Debug.Log(string.Format("전문관리번호 : {0}", recvdata.Substring(k, i - k))); //전문관리번호
                            break;
                        case 21:
                            Debug.Log(string.Format("거래일련번호 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                            break;
                    }
                    k = i + 1;

                    if (j.Equals(21))
                    {
                        break;
                    }
                }
                i++;
            }
        }

        //switch(str)
        //{
        //    case "정상승인":

        //        break;
        //}

        if (string.IsNullOrEmpty(num.Replace(" ", "")))
        {
            num = "null";
        }

        UDP_Server.SocketSend(string.Format("{0},{1},{2}", ret, num, str));
        UDP_Server._clientMessage = string.Empty;
    }

    /// <summary>
    /// 신용카드, 카카오페이, 네이버페이
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    public void PayEvent(int value, int type)
    {
        PayValue = value;

        string FS = ((char)28).ToString(); // 전문 구분자 (File Separator)
        string SendData = string.Empty;
        string num = string.Empty;
        string str = string.Empty;

        dealType = (DealType)type;
        //Debug.Log(string.Format("결제 DealType : {0}", dealType.ToString()));

        switch (dealType)
        {
            case DealType.CREDIT_APPROVAL:
                {
                    Debug.Log("신용승인");

                    SendData = "0200" + FS + "10" + FS + "C" + FS + PayValue.ToString() + FS + FS + FS + "00" + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS;
                    Debug.Log(SendData);

                    byte[] mSend = Encoding.GetEncoding(1252).GetBytes(SendData); // 요청전문 (1004원 일시불 신용승인 전문) Byte Array 저장

                    byte[] mRecv = new byte[2048]; // 응답전문 Byte Array

                    int ret = NICEVCAT(mSend, mRecv); // 함수호출
                    Debug.Log("ret : " + ret);


                    if (ret.Equals(1))
                    {
                        // 리턴값 1인 경우만 응답전문이 정상 수신
                        // 리턴값 1이면서 응답코드 “0000”인 경우만 정상 승인
                        // 리턴값 1인 경우 응답전문 수신됨

                        int i = 0;
                        int j = 0;
                        int k = 0;

                        string recvdata = Encoding.Default.GetString(mRecv); // 응답전문 recvdata에 저장
                        Debug.Log(Encoding.Default.GetString(mRecv)); // 응답전문 String

                        while (true)
                        {
                            if (recvdata.Substring(i, 1).Equals(FS))
                            {
                                j++;

                                switch (j)
                                {
                                    case 1:
                                        Debug.Log(string.Format("거래구분 : {0}", recvdata.Substring(k, i - k))); //거래구분
                                        break;
                                    case 2:
                                        Debug.Log(string.Format("거래유형 : {0}", recvdata.Substring(k, i - k))); //거래유형
                                        break;
                                    case 3:
                                        Debug.Log(string.Format("응답코드 : {0}", recvdata.Substring(k, i - k))); //응답코드
                                        break;
                                    case 4:
                                        Debug.Log(string.Format("거래금액 : {0}", recvdata.Substring(k, i - k))); //거래금액
                                        break;
                                    case 5:
                                        Debug.Log(string.Format("부가세 : {0}", recvdata.Substring(k, i - k))); //부가세
                                        break;
                                    case 6:
                                        Debug.Log(string.Format("봉사료 : {0}", recvdata.Substring(k, i - k))); //봉사료
                                        break;
                                    case 7:
                                        Debug.Log(string.Format("할부개월 : {0}", recvdata.Substring(k, i - k))); //할부개월
                                        break;
                                    case 8:
                                        Debug.Log(string.Format("승인번호 : {0}", recvdata.Substring(k, i - k))); //승인번호
                                        num = recvdata.Substring(k, i - k);
                                        break;
                                    case 9:
                                        Debug.Log(string.Format("승인일시 : {0}", recvdata.Substring(k, i - k))); //승인일시
                                        break;
                                    case 10:
                                        Debug.Log(string.Format("발급사코드 : {0}", recvdata.Substring(k, i - k))); //발급사코드
                                        break;
                                    case 11:
                                        Debug.Log(string.Format("발급사명 : {0}", recvdata.Substring(k, i - k))); //발급사명
                                        break;
                                    case 12:
                                        Debug.Log(string.Format("매입사코드 : {0}", recvdata.Substring(k, i - k))); //매입사코드
                                        break;
                                    case 13:
                                        Debug.Log(string.Format("매입사명 : {0}", recvdata.Substring(k, i - k))); //매입사명
                                        break;
                                    case 14:
                                        Debug.Log(string.Format("가맹점번호 : {0}", recvdata.Substring(k, i - k))); //가맹점번호
                                        break;
                                    case 15:
                                        Debug.Log(string.Format("승인CATID : {0}", recvdata.Substring(k, i - k))); //승인CATID
                                        break;
                                    case 16:
                                        Debug.Log(string.Format("잔액 : {0}", recvdata.Substring(k, i - k))); //잔액
                                        break;
                                    case 17:
                                        Debug.Log(string.Format("응답메시지 : {0}", recvdata.Substring(k, i - k))); //응답메시지
                                        str = recvdata.Substring(k, i - k);
                                        break;
                                    case 18:
                                        Debug.Log(string.Format("카드BIN : {0}", recvdata.Substring(k, i - k))); //카드BIN
                                        break;
                                    case 19:
                                        Debug.Log(string.Format("카드구분 : {0}", recvdata.Substring(k, i - k))); //카드구분
                                        break;
                                    case 20:
                                        Debug.Log(string.Format("전문관리번호 : {0}", recvdata.Substring(k, i - k))); //전문관리번호
                                        break;
                                    case 21:
                                        Debug.Log(string.Format("거래일련번호 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                }
                                k = i + 1;

                                if (j.Equals(21))
                                {
                                    break;
                                }
                            }
                            i++;
                        }
                    }

                    if (string.IsNullOrEmpty(num.Replace(" ", "")))
                    {
                        num = "null";
                    }

                    Debug.Log(string.Format("{0},{1},{2}", ret, num, str));

                    UDP_Server.SocketSend(string.Format("{0},{1},{2}", ret, num, str));
                    UDP_Server._clientMessage = string.Empty;

                    break;
                }
            case DealType.CREDIT_CANCELLATION:
                {
                    Debug.Log("신용취소");
                    SendData = "0420" + FS + "10" + FS + "C" + FS + PayValue.ToString() + FS + FS + FS + "00" + FS + FS + FS + "28713392" + FS + "220214" + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS;
                    break;
                }
            case DealType.KAKAO_APPROVAL: //카카오
                {
                    byte[] barcodeData = new byte[2048];
                    int dat = REQ_BARCODE(Encoding.GetEncoding(1252).GetBytes("1"), barcodeData);

                    Debug.Log("barcode data : " + Encoding.Default.GetString(barcodeData));
                    Debug.Log("return data : " + dat);

                    Debug.Log("QR페이승인"); // 030010L10040000281006027875779978448005PRO
                    SendData = "0300" + FS + "10" + FS + "L" + FS + PayValue.ToString() + FS + "0" + FS + "0" + FS + "00" + FS + FS + FS + FS + FS + FS + Encoding.Default.GetString(barcodeData) + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + "PRO" + FS + "" + FS + FS + FS + FS + FS + FS + FS;

                    Debug.Log(SendData);

                    byte[] mSend = Encoding.GetEncoding(1252).GetBytes(SendData); // 요청전문 (1004원 일시불 신용승인 전문) Byte Array 저장
                    byte[] mRecv = new byte[2048]; // 응답전문 Byte Array

                    int ret = NICEVCATB(mSend, mRecv); // 함수호출
                    Debug.Log("ret : " + ret);

                    if (ret.Equals(1))
                    {
                        // 리턴값 1인 경우만 응답전문이 정상 수신
                        // 리턴값 1이면서 응답코드 “0000”인 경우만 정상 승인
                        // 리턴값 1인 경우 응답전문 수신됨

                        int i = 0;
                        int j = 0;
                        int k = 0;

                        Debug.Log(Encoding.Default.GetString(mRecv));// 응답전문 String

                        string recvdata = Encoding.Default.GetString(mRecv); // 응답전문 recvdata에 저장

                        while (true)
                        {
                            if (recvdata.Substring(i, 1).Equals(FS))
                            {
                                j++;

                                switch (j)
                                {
                                    case 1:
                                        Debug.Log(string.Format("거래구분 : {0}", recvdata.Substring(k, i - k))); //거래구분
                                        break;
                                    case 2:
                                        Debug.Log(string.Format("거래유형 : {0}", recvdata.Substring(k, i - k))); //거래유형
                                        break;
                                    case 3:
                                        Debug.Log(string.Format("응답코드 : {0}", recvdata.Substring(k, i - k))); //응답코드
                                        break;
                                    case 4:
                                        Debug.Log(string.Format("거래금액 : {0}", recvdata.Substring(k, i - k))); //거래금액
                                        break;
                                    case 5:
                                        Debug.Log(string.Format("부가세 : {0}", recvdata.Substring(k, i - k))); //부가세
                                        break;
                                    case 6:
                                        Debug.Log(string.Format("봉사료 : {0}", recvdata.Substring(k, i - k))); //봉사료
                                        break;
                                    case 7:
                                        Debug.Log(string.Format("할부개월 : {0}", recvdata.Substring(k, i - k))); //할부개월
                                        break;
                                    case 8:
                                        Debug.Log(string.Format("승인번호 : {0}", recvdata.Substring(k, i - k))); //승인번호
                                        num = recvdata.Substring(k, i - k);
                                        break;
                                    case 9:
                                        Debug.Log(string.Format("승인일시 : {0}", recvdata.Substring(k, i - k))); //승인일시
                                        break;
                                    case 10:
                                        Debug.Log(string.Format("발급사코드 : {0}", recvdata.Substring(k, i - k))); //발급사코드
                                        break;
                                    case 11:
                                        Debug.Log(string.Format("발급사명 : {0}", recvdata.Substring(k, i - k))); //발급사명
                                        break;
                                    case 12:
                                        Debug.Log(string.Format("매입사코드 : {0}", recvdata.Substring(k, i - k))); //매입사코드
                                        break;
                                    case 13:
                                        Debug.Log(string.Format("매입사명 : {0}", recvdata.Substring(k, i - k))); //매입사명
                                        break;
                                    case 14:
                                        Debug.Log(string.Format("가맹점번호 : {0}", recvdata.Substring(k, i - k))); //가맹점번호
                                        break;
                                    case 15:
                                        Debug.Log(string.Format("승인CATID : {0}", recvdata.Substring(k, i - k))); //승인CATID
                                        break;
                                    case 16:
                                        Debug.Log(string.Format("잔액 : {0}", recvdata.Substring(k, i - k))); //잔액
                                        break;
                                    case 17:
                                        Debug.Log(string.Format("응답메시지 : {0}", recvdata.Substring(k, i - k))); //응답메시지
                                        str = recvdata.Substring(k, i - k);
                                        break;
                                    case 18:
                                        Debug.Log(string.Format("카드BIN : {0}", recvdata.Substring(k, i - k))); //카드BIN
                                        break;
                                    case 19:
                                        Debug.Log(string.Format("카드구분 : {0}", recvdata.Substring(k, i - k))); //카드구분
                                        break;
                                    case 20:
                                        Debug.Log(string.Format("전문관리번호 : {0}", recvdata.Substring(k, i - k))); //전문관리번호
                                        break;
                                    case 21:
                                        Debug.Log(string.Format("거래일련번호 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 22:
                                        Debug.Log(string.Format("발생P : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 23:
                                        Debug.Log(string.Format("가용P : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 24:
                                        Debug.Log(string.Format("발생P : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 25:
                                        Debug.Log(string.Format("캐시백가맹점 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 26:
                                        Debug.Log(string.Format("캐시백승인번호 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 27:
                                        Debug.Log(string.Format("기기번호 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                }
                                k = i + 1;

                                if (j.Equals(27))
                                {
                                    break;
                                }
                            }
                            i++;
                        }
                    }

                    if (string.IsNullOrEmpty(num.Replace(" ", "")))
                    {
                        num = "null";
                    }

                    Debug.Log(string.Format("{0},{1},{2}", ret, num, str));

                    UDP_Server.SocketSend(string.Format("{0},{1},{2}", ret, num, str));
                    UDP_Server._clientMessage = string.Empty;

                    break;
                }
            case DealType.KAKAO_CANCELLATION: 
                {
                    Debug.Log("QR페이취소"); // 052010L10040000220816281006027875779978448005PRO
                    SendData = "0520" + FS + "10" + FS + "L" + FS + PayValue.ToString() + FS + "0" + FS + "0" + FS + "00" + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + "PRO" + FS + "" + FS + FS + FS + FS + FS + FS + FS;
                    break;
                }
            case DealType.NAVER_APPROVAL: //네이버
                {
                    byte[] barcodeData = new byte[2048];
                    int dat = REQ_BARCODE(Encoding.GetEncoding(1252).GetBytes("1"), barcodeData);

                    Debug.Log("barcode data : " + Encoding.Default.GetString(barcodeData));
                    Debug.Log("return data : " + dat);

                    Debug.Log("네이버페이승인"); // BC
                    SendData = "0340" + FS + "10" + FS + "Q" + FS + PayValue.ToString() + FS + "0" + FS + "0" + FS + "00" + FS + FS + FS + FS + FS + FS + Encoding.Default.GetString(barcodeData) + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + "PRO" + FS + "" + FS + FS + FS + FS + FS + FS + FS;

                    Debug.Log(SendData);

                    byte[] mSend = Encoding.GetEncoding(1252).GetBytes(SendData); // 요청전문 (1004원 일시불 신용승인 전문) Byte Array 저장
                    byte[] mRecv = new byte[2048]; // 응답전문 Byte Array

                    int ret = NICEVCATB(mSend, mRecv); // 함수호출
                    Debug.Log("ret : " + ret);

                    if (ret.Equals(1))
                    {
                        // 리턴값 1인 경우만 응답전문이 정상 수신
                        // 리턴값 1이면서 응답코드 “0000”인 경우만 정상 승인
                        // 리턴값 1인 경우 응답전문 수신됨

                        int i = 0;
                        int j = 0;
                        int k = 0;

                        Debug.Log(Encoding.Default.GetString(mRecv));// 응답전문 String

                        string recvdata = Encoding.Default.GetString(mRecv); // 응답전문 recvdata에 저장

                        while (true)
                        {
                            if (recvdata.Substring(i, 1).Equals(FS))
                            {
                                j++;

                                switch (j)
                                {
                                    case 1:
                                        Debug.Log(string.Format("거래구분 : {0}", recvdata.Substring(k, i - k))); //거래구분
                                        break;
                                    case 2:
                                        Debug.Log(string.Format("거래유형 : {0}", recvdata.Substring(k, i - k))); //거래유형
                                        break;
                                    case 3:
                                        Debug.Log(string.Format("응답코드 : {0}", recvdata.Substring(k, i - k))); //응답코드
                                        break;
                                    case 4:
                                        Debug.Log(string.Format("거래금액 : {0}", recvdata.Substring(k, i - k))); //거래금액
                                        break;
                                    case 5:
                                        Debug.Log(string.Format("부가세 : {0}", recvdata.Substring(k, i - k))); //부가세
                                        break;
                                    case 6:
                                        Debug.Log(string.Format("봉사료 : {0}", recvdata.Substring(k, i - k))); //봉사료
                                        break;
                                    case 7:
                                        Debug.Log(string.Format("할부개월 : {0}", recvdata.Substring(k, i - k))); //할부개월
                                        break;
                                    case 8:
                                        Debug.Log(string.Format("승인번호 : {0}", recvdata.Substring(k, i - k))); //승인번호
                                        num = recvdata.Substring(k, i - k);
                                        break;
                                    case 9:
                                        Debug.Log(string.Format("승인일시 : {0}", recvdata.Substring(k, i - k))); //승인일시
                                        break;
                                    case 10:
                                        Debug.Log(string.Format("발급사코드 : {0}", recvdata.Substring(k, i - k))); //발급사코드
                                        break;
                                    case 11:
                                        Debug.Log(string.Format("발급사명 : {0}", recvdata.Substring(k, i - k))); //발급사명
                                        break;
                                    case 12:
                                        Debug.Log(string.Format("매입사코드 : {0}", recvdata.Substring(k, i - k))); //매입사코드
                                        break;
                                    case 13:
                                        Debug.Log(string.Format("매입사명 : {0}", recvdata.Substring(k, i - k))); //매입사명
                                        break;
                                    case 14:
                                        Debug.Log(string.Format("가맹점번호 : {0}", recvdata.Substring(k, i - k))); //가맹점번호
                                        break;
                                    case 15:
                                        Debug.Log(string.Format("승인CATID : {0}", recvdata.Substring(k, i - k))); //승인CATID
                                        break;
                                    case 16:
                                        Debug.Log(string.Format("잔액 : {0}", recvdata.Substring(k, i - k))); //잔액
                                        break;
                                    case 17:
                                        Debug.Log(string.Format("응답메시지 : {0}", recvdata.Substring(k, i - k))); //응답메시지
                                        str = recvdata.Substring(k, i - k);
                                        break;
                                    case 18:
                                        Debug.Log(string.Format("카드BIN : {0}", recvdata.Substring(k, i - k))); //카드BIN
                                        break;
                                    case 19:
                                        Debug.Log(string.Format("카드구분 : {0}", recvdata.Substring(k, i - k))); //카드구분
                                        break;
                                    case 20:
                                        Debug.Log(string.Format("전문관리번호 : {0}", recvdata.Substring(k, i - k))); //전문관리번호
                                        break;
                                    case 21:
                                        Debug.Log(string.Format("거래일련번호 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 22:
                                        Debug.Log(string.Format("발생P : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 23:
                                        Debug.Log(string.Format("가용P : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 24:
                                        Debug.Log(string.Format("발생P : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 25:
                                        Debug.Log(string.Format("캐시백가맹점 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 26:
                                        Debug.Log(string.Format("캐시백승인번호 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 27:
                                        Debug.Log(string.Format("기기번호 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                }
                                k = i + 1;

                                if (j.Equals(27))
                                {
                                    break;
                                }
                            }
                            i++;
                        }
                    }

                    if (string.IsNullOrEmpty(num.Replace(" ", "")))
                    {
                        num = "null";
                    }

                    Debug.Log(string.Format("{0},{1},{2}", ret, num, str));
                    UDP_Server.SocketSend(string.Format("{0},{1},{2}", ret, num, str));
                    UDP_Server._clientMessage = string.Empty;

                    break;
                }
            case DealType.NAVER_CANCELLATION:
                {
                    byte[] barcodeData = new byte[2048];
                    int dat = REQ_BARCODE(Encoding.GetEncoding(1252).GetBytes("1"), barcodeData);

                    Debug.Log("barcode data : " + Encoding.Default.GetString(barcodeData));
                    Debug.Log("return data : " + dat);

                    Debug.Log("네이버페이취소"); 
                    SendData = "0560" + FS + "10" + FS + "Q" + FS + PayValue.ToString() + FS + "0" + FS + "0" + FS + "00" + FS + "78610481" + FS + "220818" + FS + FS +
                            Encoding.Default.GetString(barcodeData) + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + "PRO" + FS + "" + FS + FS + FS + FS + FS + FS + FS;

                    Debug.Log(SendData);

                    byte[] mSend = Encoding.GetEncoding(1252).GetBytes(SendData); // 요청전문 (1004원 일시불 신용승인 전문) Byte Array 저장

                    byte[] mRecv = new byte[2048]; // 응답전문 Byte Array

                    int ret = NICEVCATB(mSend, mRecv); // 함수호출
                    Debug.Log("ret : " + ret);

                    if (ret.Equals(1))
                    {
                        // 리턴값 1인 경우만 응답전문이 정상 수신
                        // 리턴값 1이면서 응답코드 “0000”인 경우만 정상 승인
                        // 리턴값 1인 경우 응답전문 수신됨

                        int i = 0;
                        int j = 0;
                        int k = 0;

                        Debug.Log(Encoding.Default.GetString(mRecv));// 응답전문 String

                        string recvdata = Encoding.Default.GetString(mRecv); // 응답전문 recvdata에 저장

                        while (true)
                        {
                            if (recvdata.Substring(i, 1).Equals(FS))
                            {
                                j++;

                                switch (j)
                                {
                                    case 1:
                                        Debug.Log(string.Format("거래구분 : {0}", recvdata.Substring(k, i - k))); //거래구분
                                        break;
                                    case 2:
                                        Debug.Log(string.Format("거래유형 : {0}", recvdata.Substring(k, i - k))); //거래유형
                                        break;
                                    case 3:
                                        Debug.Log(string.Format("응답코드 : {0}", recvdata.Substring(k, i - k))); //응답코드
                                        break;
                                    case 4:
                                        Debug.Log(string.Format("거래금액 : {0}", recvdata.Substring(k, i - k))); //거래금액
                                        break;
                                    case 5:
                                        Debug.Log(string.Format("부가세 : {0}", recvdata.Substring(k, i - k))); //부가세
                                        break;
                                    case 6:
                                        Debug.Log(string.Format("봉사료 : {0}", recvdata.Substring(k, i - k))); //봉사료
                                        break;
                                    case 7:
                                        Debug.Log(string.Format("할부개월 : {0}", recvdata.Substring(k, i - k))); //할부개월
                                        break;
                                    case 8:
                                        Debug.Log(string.Format("승인번호 : {0}", recvdata.Substring(k, i - k))); //승인번호
                                        num = recvdata.Substring(k, i - k);
                                        break;
                                    case 9:
                                        Debug.Log(string.Format("승인일시 : {0}", recvdata.Substring(k, i - k))); //승인일시
                                        break;
                                    case 10:
                                        Debug.Log(string.Format("발급사코드 : {0}", recvdata.Substring(k, i - k))); //발급사코드
                                        break;
                                    case 11:
                                        Debug.Log(string.Format("발급사명 : {0}", recvdata.Substring(k, i - k))); //발급사명
                                        break;
                                    case 12:
                                        Debug.Log(string.Format("매입사코드 : {0}", recvdata.Substring(k, i - k))); //매입사코드
                                        break;
                                    case 13:
                                        Debug.Log(string.Format("매입사명 : {0}", recvdata.Substring(k, i - k))); //매입사명
                                        break;
                                    case 14:
                                        Debug.Log(string.Format("가맹점번호 : {0}", recvdata.Substring(k, i - k))); //가맹점번호
                                        break;
                                    case 15:
                                        Debug.Log(string.Format("승인CATID : {0}", recvdata.Substring(k, i - k))); //승인CATID
                                        break;
                                    case 16:
                                        Debug.Log(string.Format("잔액 : {0}", recvdata.Substring(k, i - k))); //잔액
                                        break;
                                    case 17:
                                        Debug.Log(string.Format("응답메시지 : {0}", recvdata.Substring(k, i - k))); //응답메시지
                                        str = recvdata.Substring(k, i - k);
                                        break;
                                    case 18:
                                        Debug.Log(string.Format("카드BIN : {0}", recvdata.Substring(k, i - k))); //카드BIN
                                        break;
                                    case 19:
                                        Debug.Log(string.Format("카드구분 : {0}", recvdata.Substring(k, i - k))); //카드구분
                                        break;
                                    case 20:
                                        Debug.Log(string.Format("전문관리번호 : {0}", recvdata.Substring(k, i - k))); //전문관리번호
                                        break;
                                    case 21:
                                        Debug.Log(string.Format("거래일련번호 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 22:
                                        Debug.Log(string.Format("발생P : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 23:
                                        Debug.Log(string.Format("가용P : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 24:
                                        Debug.Log(string.Format("발생P : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 25:
                                        Debug.Log(string.Format("캐시백가맹점 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 26:
                                        Debug.Log(string.Format("캐시백승인번호 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 27:
                                        Debug.Log(string.Format("기기번호 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                }
                                k = i + 1;

                                if (j.Equals(27))
                                {
                                    break;
                                }
                            }
                            i++;
                        }
                    }

                    if (string.IsNullOrEmpty(num.Replace(" ", "")))
                    {
                        num = "null";
                    }

                    Debug.Log(string.Format("{0},{1},{2}", ret, num, str));

                    UDP_Server.SocketSend(string.Format("{0},{1},{2}", ret, num, str));
                    UDP_Server._clientMessage = string.Empty;

                    break;
                }
            case DealType.ZERO_APPROVAL:
                {
                    byte[] barcodeData = new byte[2048];
                    int dat = REQ_BARCODE(Encoding.GetEncoding(1252).GetBytes("1"), barcodeData);

                    Debug.Log("barcode data : " + Encoding.Default.GetString(barcodeData));
                    Debug.Log("return data : " + dat);

                    Debug.Log("제로페이승인");
                    //SendData = "0300" + FS + "P1" + FS + "L" + FS + PayValue.ToString() + FS + "0" + FS + "0" + FS + "00" + FS + "" + FS + "" + FS + "" + FS + FS + FS + Encoding.Default.GetString(barcodeData) + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + FS + "ZRP" + FS + "" + FS + FS + FS + FS + FS + FS + FS;

                    SendData = "0300" + FS + "P1" + FS + "L" + FS + PayValue.ToString() + FS + "0" + FS + "0" + FS + "00" + FS + "" + FS + "" + FS + "" + FS + FS + FS + Encoding.Default.GetString(barcodeData) + FS + FS + FS + FS + "" + FS + "" + FS + FS + "ZRP" + FS + "" + FS + "" + FS + FS + FS + "" + FS;

                    Debug.Log(SendData);

                    byte[] mSend = Encoding.GetEncoding(1252).GetBytes(SendData); // 요청전문 (1004원 일시불 신용승인 전문) Byte Array 저장
                    byte[] mRecv = new byte[2048]; // 응답전문 Byte Array

                    int ret = NICEVCATB(mSend, mRecv); // 함수호출
                    Debug.Log("ret : " + ret);

                    if (ret.Equals(1))
                    {
                        // 리턴값 1인 경우만 응답전문이 정상 수신
                        // 리턴값 1이면서 응답코드 “0000”인 경우만 정상 승인
                        // 리턴값 1인 경우 응답전문 수신됨

                        int i = 0;
                        int j = 0;
                        int k = 0;

                        Debug.Log(Encoding.Default.GetString(mRecv));// 응답전문 String

                        string recvdata = Encoding.Default.GetString(mRecv); // 응답전문 recvdata에 저장

                        while (true)
                        {
                            if (recvdata.Substring(i, 1).Equals(FS))
                            {
                                j++;

                                switch (j)
                                {
                                    case 1:
                                        Debug.Log(string.Format("거래구분 : {0}", recvdata.Substring(k, i - k))); //거래구분
                                        break;
                                    case 2:
                                        Debug.Log(string.Format("거래유형 : {0}", recvdata.Substring(k, i - k))); //거래유형
                                        break;
                                    case 3:
                                        Debug.Log(string.Format("응답코드 : {0}", recvdata.Substring(k, i - k))); //응답코드
                                        break;
                                    case 4:
                                        Debug.Log(string.Format("거래금액 : {0}", recvdata.Substring(k, i - k))); //거래금액
                                        break;
                                    case 5:
                                        Debug.Log(string.Format("부가세 : {0}", recvdata.Substring(k, i - k))); //부가세
                                        break;
                                    case 6:
                                        Debug.Log(string.Format("봉사료 : {0}", recvdata.Substring(k, i - k))); //봉사료
                                        break;
                                    case 7:
                                        Debug.Log(string.Format("할부개월 : {0}", recvdata.Substring(k, i - k))); //할부개월
                                        break;
                                    case 8:
                                        Debug.Log(string.Format("승인번호 : {0}", recvdata.Substring(k, i - k))); //승인번호
                                        num = recvdata.Substring(k, i - k);
                                        break;
                                    case 9:
                                        Debug.Log(string.Format("승인일시 : {0}", recvdata.Substring(k, i - k))); //승인일시
                                        break;
                                    case 10:
                                        Debug.Log(string.Format("발급사코드 : {0}", recvdata.Substring(k, i - k))); //발급사코드
                                        break;
                                    case 11:
                                        Debug.Log(string.Format("발급사명 : {0}", recvdata.Substring(k, i - k))); //발급사명
                                        break;
                                    case 12:
                                        Debug.Log(string.Format("매입사코드 : {0}", recvdata.Substring(k, i - k))); //매입사코드
                                        break;
                                    case 13:
                                        Debug.Log(string.Format("매입사명 : {0}", recvdata.Substring(k, i - k))); //매입사명
                                        break;
                                    case 14:
                                        Debug.Log(string.Format("가맹점번호 : {0}", recvdata.Substring(k, i - k))); //가맹점번호
                                        break;
                                    case 15:
                                        Debug.Log(string.Format("승인CATID : {0}", recvdata.Substring(k, i - k))); //승인CATID
                                        break;
                                    case 16:
                                        Debug.Log(string.Format("잔액 : {0}", recvdata.Substring(k, i - k))); //잔액
                                        break;
                                    case 17:
                                        Debug.Log(string.Format("응답메시지 : {0}", recvdata.Substring(k, i - k))); //응답메시지
                                        str = recvdata.Substring(k, i - k);
                                        break;
                                    case 18:
                                        Debug.Log(string.Format("카드BIN : {0}", recvdata.Substring(k, i - k))); //카드BIN
                                        break;
                                    case 19:
                                        Debug.Log(string.Format("카드구분 : {0}", recvdata.Substring(k, i - k))); //카드구분
                                        break;
                                    case 20:
                                        Debug.Log(string.Format("전문관리번호 : {0}", recvdata.Substring(k, i - k))); //전문관리번호
                                        break;
                                    case 21:
                                        Debug.Log(string.Format("거래일련번호 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 22:
                                        Debug.Log(string.Format("발생P : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 23:
                                        Debug.Log(string.Format("가용P : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 24:
                                        Debug.Log(string.Format("발생P : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 25:
                                        Debug.Log(string.Format("캐시백가맹점 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 26:
                                        Debug.Log(string.Format("캐시백승인번호 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                    case 27:
                                        Debug.Log(string.Format("기기번호 : {0}", recvdata.Substring(k, i - k))); //거래일련번호
                                        break;
                                }
                                k = i + 1;

                                if (j.Equals(27))
                                {
                                    break;
                                }
                            }
                            i++;
                        }
                    }

                    if (string.IsNullOrEmpty(num.Replace(" ", "")))
                    {
                        num = "null";
                    }

                    Debug.Log(string.Format("{0},{1},{2}", ret, num, str));

                    UDP_Server.SocketSend(string.Format("{0},{1},{2}", ret, num, str));
                    UDP_Server._clientMessage = string.Empty;

                    break;
                }
        }
    }
}
