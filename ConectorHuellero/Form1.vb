Imports DPFP
Imports DPFP.Capture
Public Class Form1
    Implements DPFP.Capture.EventHandler

    Private Captura As DPFP.Capture.Capture
    Private Enroller As DPFP.Processing.Enrollment

    ' verifica al inciiar la capura
    Protected Overridable Sub Init()
        Try
            Captura = New Capture()
            If Not Captura Is Nothing Then
                Captura.EventHandler = Me
                Enroller = New DPFP.Processing.Enrollment()
            Else
                MessageBox.Show("no se pudo iniciar al captura")
            End If
        Catch ex As Exception
            MessageBox.Show("No se iniciar la camptura")
        End Try
    End Sub

    Protected Sub iniciarCaptura()
        If Not Captura Is Nothing Then
            Try
                Captura.StartCapture()
            Catch ex As Exception
                MessageBox.Show("no se pudo iniciar el huellero")
            End Try
        End If
    End Sub

    Protected Sub pararCaptura()
        If Not Captura Is Nothing Then
            Try
                Captura.StopCapture()
            Catch ex As Exception
                MessageBox.Show("no se pudo detener la captura")
            End Try
        End If
    End Sub


    Public Sub OnComplete(Capture As Object, ReaderSerialNumber As String, Sample As Sample) Implements EventHandler.OnComplete
        ' convierte la imagen
        Dim imagen = ConvertirSampleMapaBits(Sample)
        ' setea la imagen en el form
        ponerImagen(imagen)
    End Sub

    Public Sub OnFingerGone(Capture As Object, ReaderSerialNumber As String) Implements EventHandler.OnFingerGone

    End Sub
    ' cuanto toca con el huellero
    Public Sub OnFingerTouch(Capture As Object, ReaderSerialNumber As String) Implements EventHandler.OnFingerTouch
        ' MessageBox.Show("tocan el huellero")
    End Sub

    Public Sub OnReaderConnect(Capture As Object, ReaderSerialNumber As String) Implements EventHandler.OnReaderConnect

    End Sub

    Public Sub OnReaderDisconnect(Capture As Object, ReaderSerialNumber As String) Implements EventHandler.OnReaderDisconnect

    End Sub

    Public Sub OnSampleQuality(Capture As Object, ReaderSerialNumber As String, CaptureFeedback As CaptureFeedback) Implements EventHandler.OnSampleQuality

    End Sub
    ' forumarlio de la ventana
    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Init()
        iniciarCaptura()
    End Sub
    ' cuando se cierre la ventana
    Private Sub Main_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        pararCaptura()
    End Sub

    ' convierte sample (codigo)a imagen
    Protected Function ConvertirSampleMapaBits(ByVal Sample As DPFP.Sample) As Bitmap
        ' es una variable de tipo conversonr de un DPFP.SAMPLE
        Dim convertidor As New DPFP.Capture.SampleConversion()
        ' ES UN MAPAD E BITS
        Dim mapaBits As Bitmap = Nothing
        convertidor.ConvertToPicture(Sample, mapaBits)
        Return mapaBits
    End Function

    'mostrar la iamgen en el cuadro de imagen
    Private Sub ponerImagen(ByVal bmp)
        imagenHuella.Image = bmp
    End Sub
    'extre las caracteristicas de la huella para guardarla en la base de datos
    Protected Function extraerCaracteristica(ByVal Sample As DPFP.Sample, ByVal Purpose As DPFP.Processing.DataPurpose) As DPFP.FeatureSet
        Dim extractor As New DPFP.Processing.FeatureExtraction
        Dim feedback As DPFP.Capture.CaptureFeedback = DPFP.Capture.CaptureFeedback.None
        Dim caracteristicas As New DPFP.FeatureSet()
        ' verfica el estado de la huella
        extractor.CreateFeatureSet(Sample, Purpose, feedback, caracteristicas)
        If (feedback = DPFP.Capture.CaptureFeedback.Good) Then
            Return caracteristicas
        Else
            Return Nothing
        End If

    End Function

    Protected Sub Procesar(ByVal Sample As DPFP.Sample)
        Dim caracteristicas As DPFP.FeatureSet = extraerCaracteristica(Sample, DPFP.Processing.DataPurpose.Enrollment)
        If (Not caracteristicas Is Nothing) Then
            Try
                Enroller.AddFeatures(caracteristicas)
            Finally
                Select Case Enroller.TemplateStatus
                    Case DPFP.Processing.Enrollment.Status.Ready
                        pararCaptura()
                    Case DPFP.Processing.Enrollment.Status.Failed
                        Enroller.Clear()
                        pararCaptura()
                        iniciarCaptura()
                End Select

            End Try
        End If
    End Sub


End Class
