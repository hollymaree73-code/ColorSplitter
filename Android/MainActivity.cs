using Android.App;
using Android.OS;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace ColorSplitter.Android;

[Activity(
    Label = "Color Splitter",
    MainLauncher = true,
    Exported = true,
    Theme = "@android:style/Theme.Material.Light.NoActionBar",
    ConfigurationChanges = global::Android.Content.PM.ConfigChanges.Orientation |
                           global::Android.Content.PM.ConfigChanges.ScreenSize |
                           global::Android.Content.PM.ConfigChanges.UiMode)]
public sealed class MainActivity : Activity
{
    private const int PickImageRequest = 4101;

    private ImageView? _preview;
    private TextView? _status;
    private Bitmap? _loadedImage;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var page = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };

        page.SetPadding(32, 36, 32, 24);
        page.SetBackgroundColor(Color.Rgb(18, 20, 26));

        var title = new TextView(this)
        {
            Text = "Color Splitter",
            TextSize = 28
        };

        title.SetTextColor(Color.White);
        page.AddView(title);

        var subtitle = new TextView(this)
        {
            Text = "Separate artwork into clean colour layers",
            TextSize = 14
        };

        subtitle.SetTextColor(Color.Rgb(180, 186, 198));
        page.AddView(subtitle);

        var importButton = new Button(this)
        {
            Text = "Choose image"
        };

        importButton.Click += (_, _) => OpenImagePicker();

        var buttonParams = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.WrapContent)
        {
            TopMargin = 28,
            BottomMargin = 16
        };

        page.AddView(importButton, buttonParams);

        _preview = new ImageView(this);
        _preview.SetAdjustViewBounds(true);

        _preview.SetScaleType(ImageView.ScaleType.FitCenter);
        _preview.SetBackgroundColor(Color.Rgb(35, 39, 48));

        var previewParams = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            0,
            1f
        );

        page.AddView(_preview, previewParams);

        _status = new TextView(this)
        {
            Text = "Choose an image to get started.",
            TextSize = 14
        };

        _status.SetTextColor(Color.Rgb(200, 205, 215));

        var statusParams = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.WrapContent)
        {
            TopMargin = 16
        };

        page.AddView(_status, statusParams);

        SetContentView(page);
    }

    private void OpenImagePicker()
    {
        var intent = new Intent(Intent.ActionGetContent);
        intent.SetType("image/*");

        StartActivityForResult(
            Intent.CreateChooser(intent, "Select an image"),
            PickImageRequest
        );
    }

    protected override void OnActivityResult(
        int requestCode,
        Result resultCode,
        Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (requestCode != PickImageRequest ||
            resultCode != Result.Ok ||
            data?.Data is null)
            return;

        try
        {
            using var stream = ContentResolver?.OpenInputStream(data.Data);

            if (stream is null)
                throw new InvalidOperationException(
                    "Android could not open the selected image.");

            _loadedImage?.Dispose();
            _loadedImage = BitmapFactory.DecodeStream(stream);

            if (_loadedImage is null)
                throw new InvalidOperationException(
                    "The selected file could not be decoded.");

            _preview?.SetImageBitmap(_loadedImage);

            if (_status is not null)
            {
                _status.Text =
                    $"Loaded {_loadedImage.Width} × {_loadedImage.Height} image.";
            }
        }
        catch (Exception ex)
        {
            Toast.MakeText(
                this,
                ex.Message,
                ToastLength.Long)?.Show();
        }
    }

    protected override void OnDestroy()
    {
        _loadedImage?.Dispose();
        _loadedImage = null;

        base.OnDestroy();
    }
}
