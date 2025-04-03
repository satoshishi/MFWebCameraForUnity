install git url
```
https://github.com/satoshishi/MFWebCameraForUnity.git?path=UnityProject/Assets/MediaFoundation WebCamera
```

## 概要

```
Get-PnpDevice | Where-Object {$_.FriendlyName -match 'Camera|Webcam|USB\ ビデオ'} | Select-Object FriendlyName,InstanceId

FriendlyName                    InstanceId
------------                    ----------
HogeHoge Webcam              USB\VID_xxx&PID_xxxx&MI_xxxx

```

あたりで取得できるInstanceIdからWebカメラを指定して映像を取得する

```

private MediaFoundationWebCamera webcam;

this.renderer.texture = webcam.Texture;

```

↑のような感じでRenderTextureとして映像にアクセスできる。

## 使い方

- `UnityMainThreadDispatcher`を1つアタッチする。`MediaFoundation WebCamera`を同時起動したいカメラの台数分アタッチする。
- `MediaFoundation WebCamera`の`Instance Id`に対象カメラのInstance Idを指定する。Width, Heightに対象カメラの解像度を指定する

## 補足
- [このへん](https://www.logicool.co.jp/ja-jp/shop/p/c920-pro-hd-webcam.960-001261)や、[このへん](https://www.amazon.co.jp/HDMI%E5%90%8C%E6%99%82%E5%87%BA%E5%8A%9B%E3%82%AB%E3%83%A1%E3%83%A9-2-8-12mm-4%E5%80%8D%E6%89%8B%E5%8B%95%E3%82%BA%E3%83%BC%E3%83%A0%E3%83%AC%E3%83%B3%E3%82%BA-%E3%82%AF%E3%83%AD%E3%83%BC%E3%82%BA%E3%82%A2%E3%83%83%E3%83%97USB%E3%83%93%E3%83%87%E3%82%AA%E3%82%AB%E3%83%A1%E3%83%A9-TV%E3%83%97%E3%83%AD%E3%82%B8%E3%82%A7%E3%82%AF%E3%82%BF%E3%83%BC%E3%83%A2%E3%83%8B%E3%82%BF%E3%83%BC%E7%94%A3%E6%A5%AD%E7%94%A8%E3%82%A2%E3%83%97%E3%83%AA%E3%82%B1%E3%83%BC%E3%82%B7%E3%83%A7%E3%83%B3%E7%94%A8/dp/B0D2N92P1W/ref=sr_1_12?dib=eyJ2IjoiMSJ9.OqM2WEhHMK6OYjewcL_p0Bbe6BYLPsXuru6o74Sph8fw6QtVNlFgrQWFUuRyS_dGCJZNpOWz8lcxVLEqeBj18ckrup8s_QI1TPnCibRgNOoVR7SypkMTX3k8nNv7olR5vEF65QVEAfBKxC7Oi_tGUYrITMeGgA4M09ljkekX6Q8q0Xh_dNjjfllh_VFfQFszCwlD3ABZcVDJ95GekqBO0QiNLfCULXAPu4RTKTAjd-1KcDgeXVtUBd2a4MMcze7APa9LlBlzba8a2hDtjQ15A0o4QWiN9uTaV48GxaCptnI.k8Wjp47GnV5DIMpkpWVvC04NN7rWG8_xy4lsGPO8Lgg&dib_tag=se&qid=1743668173&refinements=p_n_feature_seventeen_browse-bin%3A10540656051&s=computers&sr=1-12&th=1)は動作を確認しているが、カメラによっては画像フォーマットの関係でうまく表示できないかもしれない。
- 4台同時起動まで動作確認済み
