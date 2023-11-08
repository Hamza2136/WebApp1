var dtable;
$(document).ready(function () {
    dtable = $('#mytable').DataTable({
        "ajax": {
            "url": "/Admin/Product/AllProducts"
        },
        "columns": [
            { "data": 'name' },
            { "data": 'description' },
            { "data": 'price' },
            { "data": 'category.name' },
            {
                "data": 'imageUrl',
                "render": function (data) {
                    return `<img src="${data}" alt="Product Image" class=".img-thumbnail" style="max-width: 100px; max-height: 100px;" />`;
                }
            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <a href="/admin/product/addupdate?id=${data}"><i class="bi bi-pencil-square"></i></a>
                    <a onclick=RemoveProduct("/admin/product/delete/${data}")><i class="bi bi-trash"></i></a>
                    `;
                }
            }
        ]
    })
})
function RemoveProduct(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {

            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        dtable.ajax.reload();
                        toastr.success(data.message)
                    }
                    else {
                        toastr.error(data.message)
                    }
                }
            });
        }
    });
}