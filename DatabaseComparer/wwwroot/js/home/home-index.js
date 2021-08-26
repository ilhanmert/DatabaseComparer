function ClearInput(inputName) {
    $('#' + inputName).val('');
}

function ClearCheckbox(inputName) {
    $('input[name=' + inputName + ']').prop('checked', false);
}

function ClearAllFilter() {
    ClearInput('Name');
    ClearCheckbox('Type');
}

function GetDatasByFilters(page = 1, pagesize = 50) {
    if (page < 1 || pagesize < 0) {
        return;
    }
    var filters = {};
    //filtre nesnesi olusturuluyor
    $("#filters").serializeArray().map(function (x) {
        if (filters[x.name]) {
            var newValue = []
            var fvalue = filters[x.name];
            if (Array.isArray(fvalue)) {
                for (var i = 0; i < fvalue.length; i++) {
                    newValue.push(fvalue[i]);
                }
            }
            else {
                newValue.push(fvalue);
            }
            newValue.push(x.value);
            filters[x.name] = newValue;

        }
        else {
            filters[x.name] = x.value;
        }
    });
    var defaultpagesize = 50;
    var selectpagesize = parseInt($('#select_page_size').val());
    if (selectpagesize > defaultpagesize)
        defaultpagesize = selectpagesize;
    if (pagesize > defaultpagesize)
        defaultpagesize = pagesize;
    Swal.fire({
        title: 'Karşılaştırmalar Getiriliyor.',
        onBeforeOpen: () => {
            Swal.showLoading()
        }
    });
    $.ajax({//ajax istegi olusturuluyor
        method: 'POST',
        url: '/Home/GetList',
        data: { filters: filters, page: page, pagesize: defaultpagesize }
    }).done(function (result) {//eger ajax function duzgun calismis ise done functionu active oluyor

        $('#content').html(result);
        Swal.close();
    });
}

function copyToClipboard(element) {
    navigator.clipboard.writeText($(element).text())
        .then(() => {
            alert("Kopyalandı!");
        })
        .catch(err => {
            alert("Kopyalanırken Bir Hata Meydana Geldi!" + err);
        });
}

$(document).ready(function () {
    GetDatasByFilters();
});