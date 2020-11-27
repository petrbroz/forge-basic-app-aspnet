Autodesk.Viewing.Initializer({ getAccessToken }, async function () {
    const config = { extensions: ['Autodesk.Viewing.MarkupsCore', 'Autodesk.Viewing.MarkupsGui'] };
    const viewer = new Autodesk.Viewing.GuiViewer3D(document.getElementById('preview'), config);
    viewer.start();
    viewer.setTheme('light-theme');
    await viewer.loadExtension('Autodesk.PDF');
    viewer.loadModel('/sample.pdf', { page: 1 });

    // On the "bake" button click, send markups to our server and wait for results
    document.getElementById('bake').addEventListener('click', async function () {
        const markupsExt = viewer.getExtension('Autodesk.Viewing.MarkupsCore');
        const metadata = viewer.model.getData().metadata;
        let data = new FormData();
        data.append('page_number', 1);
        data.append('page_width', metadata.page_dimensions.page_width);
        data.append('page_height', metadata.page_dimensions.page_height);
        data.append('page_units', metadata.page_dimensions.page_units);
        data.append('markups', markupsExt.generateData());
        const resp = await fetch('/api/markups', {
            method: 'POST',
            body: data
        });
        if (resp.ok) {
            const pdf = await resp.blob();
            const url = window.URL.createObjectURL(pdf);
            window.open(url, 'sample.pdf');
        } else {
            alert('Could not add markups into the PDF. See the console for more details.');
            console.error(await resp.text());
        }
    });
});

/**
 * Retrieves access token required for viewing models.
 * @async
 * @param {function} callback Callback function to be called with access token and expiration time (in seconds).
 */
async function getAccessToken(callback) {
    const resp = await fetch('/api/auth/token');
    if (resp.ok) {
        const { access_token, expires_in } = await resp.json();
        callback(access_token, expires_in);
    } else {
        alert('Could not obtain access token. See the console for more details.');
        console.error(await resp.text());
    }
}
